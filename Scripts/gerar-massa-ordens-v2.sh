#!/bin/bash
# Script para gerar massa de ordens de serviço reais, autenticando e usando dados válidos
# Uso: ./gerar-massa-ordens-v2.sh <COUNT> <URL_API>
# Exemplo: ./gerar-massa-ordens-v2.sh 100 http://localhost:5000

COUNT=${1:-10}
API_URL=${2:-http://localhost:5000}
USER_EMAIL="teste$(date +%s)@exemplo.com"
USER_NAME="usuarioteste"
USER_PASSWORD="Teste@123"

# 1. Criar usuário
REG_PAYLOAD=$(cat <<EOF
{
  "nomeUsuario": "$USER_NAME",
  "email": "$USER_EMAIL",
  "senha": "$USER_PASSWORD"
}
EOF
)
REG_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API_URL/api/autenticacao/registro" -H "Content-Type: application/json" -d "$REG_PAYLOAD")
echo "Status criação usuário: $REG_STATUS"

# 2. Login
LOGIN_PAYLOAD=$(cat <<EOF
{
  "nomeUsuario": "$USER_NAME",
  "senha": "$USER_PASSWORD"
}
EOF
)
TOKEN=$(curl -s -X POST "$API_URL/api/autenticacao/login" -H "Content-Type: application/json" -d "$LOGIN_PAYLOAD" | grep -o '"token"\s*:\s*"[^"]*"' | cut -d '"' -f4)
if [ -z "$TOKEN" ]; then
  echo "Falha ao obter token JWT."
  exit 1
fi

# 3. Buscar clientes, serviços e peças
CLIENTES=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_URL/api/clientes")
SERVICOS=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_URL/api/servicos")
PECAS=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_URL/api/pecas")

# Extrair IDs válidos
CLIENTE_ID=$(echo "$CLIENTES" | grep -o '"id"\s*:\s*[0-9]*' | head -1 | grep -o '[0-9]*')
SERVICO_ID=$(echo "$SERVICOS" | grep -o '"id"\s*:\s*[0-9]*' | head -1 | grep -o '[0-9]*')
PECA_ID=$(echo "$PECAS" | grep -o '"id"\s*:\s*[0-9]*' | head -1 | grep -o '[0-9]*')

if [ -z "$CLIENTE_ID" ] || [ -z "$SERVICO_ID" ] || [ -z "$PECA_ID" ]; then
  echo "Não foi possível obter IDs válidos de cliente, serviço ou peça."
  exit 1
fi

echo "Enviando $COUNT ordens de serviço para $API_URL/api/ordensservico ..."
for i in $(seq 1 $COUNT); do
  PLACA="ABC$(printf '%04d' $((RANDOM % 10000)))"
  ANO=$((2020 + (RANDOM % 5)))
  JSON=$(cat <<EOF
{
  "clienteId": $CLIENTE_ID,
  "veiculoPlaca": "$PLACA",
  "veiculoMarcaModelo": "Modelo Teste",
  "veiculoAnoFabricacao": $ANO,
  "servicosIds": [$SERVICO_ID],
  "pecas": [
    { "idPeca": $PECA_ID, "quantidade": 1 }
  ]
}
EOF
)
  curl -s -o /dev/null -w "%{http_code}\n" -X POST "$API_URL/api/ordensservico" -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "$JSON"
done

echo "\nConcluído. Usuário: $USER_NAME | Email: $USER_EMAIL | Senha: $USER_PASSWORD"

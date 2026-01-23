using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OficinaCardozo.Application.Services;
using OficinaCardozo.Application.DTOs;
using OficinaCardozo.API.Controllers;
using FluentAssertions;
using OficinaCardozo.Application.Interfaces;

namespace OficinaCardozo.Tests.UnitTests.Controllers;

public class OrdemServicoControllerMainTests
{
    private readonly Mock<IOrdemServicoService> _mockOrdemServicoService;
    private readonly OrdensServicoController _controller;

    public OrdemServicoControllerMainTests()
    {
        _mockOrdemServicoService = new Mock<IOrdemServicoService>();
        _controller = new OrdensServicoController(
            _mockOrdemServicoService.Object,
            Mock.Of<IServicoService>(),
            Mock.Of<IPecaService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrdensServicoController>>()
        );
    }

    #region  Cria��o de Ordem

    [Fact]
    public async Task CreateOrdemServico_ComSucesso_DeveRetornarCreated()
    {
        // Arrange
        var createDto = new CreateOrdemServicoDto();
        var ordemServico = new OrdemServicoDto { Id = 1 };

        _mockOrdemServicoService
            .Setup(x => x.CreateOrdemServicoComOrcamentoAsync(createDto))
            .ReturnsAsync(ordemServico);

        // Act
        var result = await _controller.CreateOrdemServicoComOrcamento(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(ordemServico);
    }

    [Fact]
    public async Task CreateOrdemServico_ClienteNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var createDto = new CreateOrdemServicoDto();
        _mockOrdemServicoService
            .Setup(x => x.CreateOrdemServicoComOrcamentoAsync(createDto))
            .ThrowsAsync(new KeyNotFoundException("Cliente n�o encontrado"));

        // Act
        var result = await _controller.CreateOrdemServicoComOrcamento(createDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region  Consultas

    [Fact]
    public async Task GetById_ComIdValido_DeveRetornarOrdem()
    {
        // Arrange
        var ordemServico = new OrdemServicoDto { Id = 1 };
        _mockOrdemServicoService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(ordemServico);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(ordemServico);
    }

    [Fact]
    public async Task GetById_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        _mockOrdemServicoService.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((OrdemServicoDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region  Fluxo de Diagn�stico

    [Fact]
    public async Task IniciarDiagnostico_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        var orcamentoResumo = new OrcamentoResumoDto { Id = 1, StatusDescricao = "Diagnostico iniciado" };
        _mockOrdemServicoService.Setup(x => x.IniciarDiagnosticoAsync(1)).ReturnsAsync(orcamentoResumo);

        // Act
        var result = await _controller.IniciarDiagnostico(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(orcamentoResumo);
    }

    [Fact]
    public async Task AprovarOrcamento_ComAprovacao_DeveRetornarOk()
    {
        // Arrange
        var aprovarDto = new AprovarOrcamentoDto { IdOrcamento = 1, Aprovado = true };
        var orcamentoResumo = new OrcamentoResumoDto { StatusDescricao = "Aprovado" };

        _mockOrdemServicoService
            .Setup(x => x.AprovarOrcamentoAsync(aprovarDto))
            .ReturnsAsync(orcamentoResumo);

        // Act
        var result = await _controller.AprovarOrcamento(aprovarDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(orcamentoResumo);
    }

    #endregion
}
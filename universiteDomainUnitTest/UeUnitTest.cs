using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Exceptions.UeExceptions;
using UniversiteDomain.UseCases.UeUseCases.Create;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

namespace UniversiteDomainUnitTests;

public class UeUnitTest
{
    private Mock<IUeRepository> _mockUe;
    private Mock<IParcoursRepository> _mockParcours;
    private Mock<IRepositoryFactory> _mockFactory;

    [SetUp]
    public void Setup()
    {
        // Initialisation commune à tous les tests (Create et AddDansParcours)
        _mockUe = new Mock<IUeRepository>();
        _mockParcours = new Mock<IParcoursRepository>();
        _mockFactory = new Mock<IRepositoryFactory>();

        _mockFactory.Setup(facto => facto.UeRepository()).Returns(_mockUe.Object);
        _mockFactory.Setup(facto => facto.ParcoursRepository()).Returns(_mockParcours.Object);
    }

    // =========================================================================
    // TESTS : CreateUeUseCase
    // =========================================================================

    [Test]
    public async Task CreateUeUseCase()
    {
        long idUe = 1;
        string numeroUe = "INFO101";
        string nomUe = "Algorithmique et Programmation";

        // On crée l'UE qui doit être ajoutée en base
        Ue ueAvant = new Ue { NumeroUe = numeroUe, Intitule = nomUe };

        // On dit à ce mock que l'UE n'existe pas déjà (la recherche par numéro renvoie une liste vide)
        _mockUe
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>());

        // On lui dit que l'ajout renvoie une UE avec l'Id 1 généré
        Ue ueFinal = new Ue { Id = idUe, NumeroUe = numeroUe, Intitule = nomUe };
        _mockUe
            .Setup(repo => repo.CreateAsync(ueAvant))
            .ReturnsAsync(ueFinal);

        // Création du use case
        CreateUeUseCase useCase = new CreateUeUseCase(_mockFactory.Object);

        // Appel du use case
        var ueTeste = await useCase.ExecuteAsync(ueAvant);

        // Vérification du résultat
        Assert.That(ueTeste.Id, Is.EqualTo(ueFinal.Id));
        Assert.That(ueTeste.NumeroUe, Is.EqualTo(ueFinal.NumeroUe));
        Assert.That(ueTeste.Intitule, Is.EqualTo(ueFinal.Intitule));
    }
    

    [Test]
    public async Task AddUeDansParcoursUseCase_AjoutUneUe_Succes()
    {
        long idUe = 1;
        long idParcours = 3;

        Ue ue = new Ue { Id = idUe, NumeroUe = "INFO101", Intitule = "Algo" };
        Parcours parcours = new Parcours { Id = idParcours, NomParcours = "Licence Info", UesEnseignees = null };

        _mockUe.Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        _mockParcours.Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours> { parcours });

        Parcours parcoursFinal = new Parcours
            { Id = idParcours, NomParcours = "Licence Info", UesEnseignees = new List<Ue> { ue } };

        _mockParcours.Setup(repo => repo.AddUeAsync(idParcours, idUe))
            .ReturnsAsync(parcoursFinal);

        AddUeDansParcoursUseCase useCase = new AddUeDansParcoursUseCase(_mockFactory.Object);

        var resultat = await useCase.ExecuteAsync(idParcours, idUe);

        Assert.That(resultat.Id, Is.EqualTo(idParcours));
        Assert.That(resultat.UesEnseignees, Is.Not.Null);
        Assert.That(resultat.UesEnseignees.Count, Is.EqualTo(1));
        Assert.That(resultat.UesEnseignees[0].Id, Is.EqualTo(idUe));
    }

    [Test]
    public async Task AddUeDansParcoursUseCase_AjoutPlusieursUes_Succes()
    {
        long idParcours = 3;
        List<Ue> ues = new List<Ue>
        {
            new Ue { Id = 1, NumeroUe = "UE1" },
            new Ue { Id = 2, NumeroUe = "UE2" }
        };
        long[] idUes = ues.Select(x => x.Id).ToArray();

        Parcours parcours = new Parcours { Id = idParcours, UesEnseignees = null };

        _mockUe.Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { new Ue() });
    }
}
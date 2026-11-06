using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;

namespace UniversiteDomainUnitTests;

public class EtudiantUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CreateEtudiantUseCase()
    {
        long id = 1;
        String numEtud = "et1";
        string nom = "Durant";
        string prenom = "Jean";
        string email = "jean.durant@etud.u-picardie.fr";
        
        // On crée l'étudiant qui doit être ajouté en base
        Etudiant etudiantAvant = new Etudiant { NumEtud = numEtud, Nom = nom, Prenom = prenom, Email = email };
        
        // On initialise une fausse datasource qui va simuler un EtudiantRepository
        var mockEtudiant = new Mock<IEtudiantRepository>();
        
        // Simulation de la fonction FindByCondition : on simule que l'étudiant n'existe pas déjà
        mockEtudiant
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant>());
        
        // Simulation de la fonction Create : on lui dit que l'ajout renvoie l'étudiant avec son Id généré
        Etudiant etudiantFinal = new Etudiant { Id = id, NumEtud = numEtud, Nom = nom, Prenom = prenom, Email = email };
        mockEtudiant
            .Setup(repo => repo.CreateAsync(etudiantAvant))
            .ReturnsAsync(etudiantFinal);
        
        // Création d'une fausse factory qui contient le faux repository d'étudiant
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto => facto.EtudiantRepository()).Returns(mockEtudiant.Object);
        
        // Création du use case en utilisant le mock de la factory
        CreateEtudiantUseCase useCase = new CreateEtudiantUseCase(mockFactory.Object);
        
        // Appel du use case
        var etudiantTeste = await useCase.ExecuteAsync(etudiantAvant);
        
        // Vérification du résultat
        Assert.That(etudiantTeste.Id, Is.EqualTo(etudiantFinal.Id));
        Assert.That(etudiantTeste.NumEtud, Is.EqualTo(etudiantFinal.NumEtud));
        Assert.That(etudiantTeste.Nom, Is.EqualTo(etudiantFinal.Nom));
        Assert.That(etudiantTeste.Prenom, Is.EqualTo(etudiantFinal.Prenom));
        Assert.That(etudiantTeste.Email, Is.EqualTo(etudiantFinal.Email));
    }
}
using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.UseCases.NoteUseCases.Create;

namespace UniversiteDomainUnitTests;

public class NoteUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CreateNoteUseCase()
    {
        long idEtudiant = 1;
        long idUe = 10;
        float valeurNote = 15.5f;
        
        // On crée la note qui doit être ajoutée en base
        Note noteAvant = new Note { EtudiantId = idEtudiant, UeId = idUe, Valeur = valeurNote };
        
        // On initialise des faux repositories
        var mockNote = new Mock<INoteRepository>();
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockUe = new Mock<IUeRepository>();
        var mockParcours = new Mock<IParcoursRepository>();
        
        // Simulation du parcours et de l'UE
        Ue ue = new Ue { Id = idUe, NumeroUe = "INFO101" };
        Parcours parcours = new Parcours { Id = 3 };
        parcours.UesEnseignees = new List<Ue>();
        parcours.UesEnseignees.Add(ue);
        
        // Simulation de l'étudiant
        Etudiant etudiant = new Etudiant { Id = idEtudiant, ParcoursSuivi = parcours };
        List<Etudiant> etudiants = new List<Etudiant>();
        etudiants.Add(etudiant);
        mockEtudiant
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(etudiants);
            
        // Simulation de l'UE
        List<Ue> ues = new List<Ue>();
        ues.Add(ue);
        mockUe
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(ues);

        // Simulation du parcours
        List<Parcours> parcourses = new List<Parcours>();
        parcourses.Add(parcours);
        mockParcours
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(parcourses);

        // On dit à ce mock que la note n'existe pas déjà
        mockNote
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync((List<Note>)null);

        // On lui dit que l'ajout renvoie une note finale
        Note noteFinale = new Note { EtudiantId = idEtudiant, UeId = idUe, Valeur = valeurNote };
        mockNote.Setup(repo => repo.CreateAsync(noteAvant)).ReturnsAsync(noteFinale);
        
        // Création d'une fausse factory qui contient les faux repositories
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto => facto.NoteRepository()).Returns(mockNote.Object);
        mockFactory.Setup(facto => facto.EtudiantRepository()).Returns(mockEtudiant.Object);
        mockFactory.Setup(facto => facto.UeRepository()).Returns(mockUe.Object);
        mockFactory.Setup(facto => facto.ParcoursRepository()).Returns(mockParcours.Object);
        
        // Création du use case en utilisant le mock comme datasource
        CreateNoteUseCase useCase = new CreateNoteUseCase(mockFactory.Object);
        
        // Appel du use case
        var noteTeste = await useCase.ExecuteAsync(noteAvant);
        
        // Vérification du résultat
        Assert.That(noteTeste.EtudiantId, Is.EqualTo(noteFinale.EtudiantId));
        Assert.That(noteTeste.UeId, Is.EqualTo(noteFinale.UeId));
        Assert.That(noteTeste.Valeur, Is.EqualTo(noteFinale.Valeur));
    }
    
    [Test]
    public void CreateNoteUseCase_NoteInvalide()
    {
        // On crée une note invalide
        Note noteNegative = new Note { EtudiantId = 1, UeId = 10, Valeur = -2f };
        
        // Création d'une fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();
        
        // Création du use case en utilisant le mock comme datasource
        CreateNoteUseCase useCase = new CreateNoteUseCase(mockFactory.Object);
        
        // Vérification que l'exception est bien levée
        Assert.ThrowsAsync<InvalidNoteValueException>(async () => await useCase.ExecuteAsync(noteNegative));
    }

    [Test]
    public void CreateNoteUseCase_NoteExisteDeja()
    {
        long idEtudiant = 1;
        long idUe = 10;
        Note noteAvant = new Note { EtudiantId = idEtudiant, UeId = idUe, Valeur = 12f };
        
        // On initialise des faux repositories
        var mockNote = new Mock<INoteRepository>();
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockUe = new Mock<IUeRepository>();
        
        List<Etudiant> etudiants = new List<Etudiant>();
        etudiants.Add(new Etudiant { Id = idEtudiant });
        mockEtudiant
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(etudiants);
            
        List<Ue> ues = new List<Ue>();
        ues.Add(new Ue { Id = idUe });
        mockUe
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(ues);

        // On simule qu'une note existe DÉJÀ dans le repository
        List<Note> notes = new List<Note>();
        notes.Add(new Note { EtudiantId = idEtudiant, UeId = idUe, Valeur = 10f });
        mockNote
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(notes);

        // Création d'une fausse factory qui contient les faux repositories
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto => facto.NoteRepository()).Returns(mockNote.Object);
        mockFactory.Setup(facto => facto.EtudiantRepository()).Returns(mockEtudiant.Object);
        mockFactory.Setup(facto => facto.UeRepository()).Returns(mockUe.Object);
        
        // Création du use case
        CreateNoteUseCase useCase = new CreateNoteUseCase(mockFactory.Object);
        
        // Vérification de l'exception
        Assert.ThrowsAsync<DuplicateNoteException>(async () => await useCase.ExecuteAsync(noteAvant));
    }

    [Test]
    public void CreateNoteUseCase_UeNonDansParcours()
    {
        long idEtudiant = 1;
        long idUe = 10;
        Note noteAvant = new Note { EtudiantId = idEtudiant, UeId = idUe, Valeur = 12f };
        
        // On initialise des faux repositories
        var mockNote = new Mock<INoteRepository>();
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockUe = new Mock<IUeRepository>();
        
        // Le parcours contient l'UE 99, mais la note concerne l'UE 10
        Parcours parcours = new Parcours { Id = 3 };
        parcours.UesEnseignees = new List<Ue>();
        parcours.UesEnseignees.Add(new Ue { Id = 99 });
        
        Etudiant etudiant = new Etudiant { Id = idEtudiant, ParcoursSuivi = parcours };
        List<Etudiant> etudiants = new List<Etudiant>();
        etudiants.Add(etudiant);
        
        mockEtudiant
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(etudiants);
            
        List<Ue> ues = new List<Ue>();
        ues.Add(new Ue { Id = idUe });
        mockUe
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(ues);

        mockNote
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync((List<Note>)null);

        // Création d'une fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto => facto.NoteRepository()).Returns(mockNote.Object);
        mockFactory.Setup(facto => facto.EtudiantRepository()).Returns(mockEtudiant.Object);
        mockFactory.Setup(facto => facto.UeRepository()).Returns(mockUe.Object);
        
        // Création du use case
        CreateNoteUseCase useCase = new CreateNoteUseCase(mockFactory.Object);
        
        // Vérification de l'exception
        Assert.ThrowsAsync<UeNotInParcoursException>(async () => await useCase.ExecuteAsync(noteAvant));
    }
}
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Create;

public class CreateNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(long idEtudiant, long idUe, float valeur)
    {
        var note = new Note { EtudiantId = idEtudiant, UeId = idUe, Valeur = valeur };
        return await ExecuteAsync(note);
    }

    public async Task<Note> ExecuteAsync(Note note)
    {
        await CheckBusinessRules(note);
        Note noteCree = await repositoryFactory.NoteRepository().CreateAsync(note);
        repositoryFactory.SaveChangesAsync().Wait();
        return noteCree;
    }

    private async Task CheckBusinessRules(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // --------------------------------------------------------
        // Règle 1 : Une note est comprise entre 0 et 20
        // --------------------------------------------------------
        if (note.Valeur < 0 || note.Valeur > 20)
        {
            throw new InvalidNoteValueException($"La note {note.Valeur} est invalide. Elle doit être comprise entre 0 et 20.");
        }

        // Vérification de l'existence de l'étudiant
        var etudiants = await repositoryFactory.EtudiantRepository().FindByConditionAsync(e => e.Id.Equals(note.EtudiantId));
        if (etudiants == null || etudiants.Count == 0) 
            throw new EtudiantNotFoundException(note.EtudiantId.ToString());
        
        var etudiant = etudiants[0];

        // Vérification de l'existence de l'UE
        var ues = await repositoryFactory.UeRepository().FindByConditionAsync(u => u.Id.Equals(note.UeId));
        if (ues == null || ues.Count == 0) 
            throw new UeNotFoundException(note.UeId.ToString());

        // --------------------------------------------------------
        // Règle 2 : Un étudiant n'a qu'une note au maximum par Ue
        // --------------------------------------------------------
        var notesExistantes = await repositoryFactory.NoteRepository().FindByConditionAsync(n => n.EtudiantId.Equals(note.EtudiantId) && n.UeId.Equals(note.UeId));
        if (notesExistantes is { Count: > 0 })
        {
            throw new DuplicateNoteException($"L'étudiant ID {note.EtudiantId} a déjà une note enregistrée pour l'UE ID {note.UeId}.");
        }

        // --------------------------------------------------------
        // Règle 3 : Un étudiant ne peut avoir une note que dans une Ue du parcours dans lequel il est inscrit
        // --------------------------------------------------------
        if (etudiant.ParcoursSuivi == null)
        {
            throw new UeNotInParcoursException($"L'étudiant ID {note.EtudiantId} n'est inscrit dans aucun parcours et ne peut donc pas recevoir de note.");
        }

        // Récupération du parcours de l'étudiant pour vérifier les UEs enseignées
        var parcoursList = await repositoryFactory.ParcoursRepository().FindByConditionAsync(p => p.Id.Equals(etudiant.ParcoursSuivi));
        if (parcoursList == null || parcoursList.Count == 0) 
            throw new ParcoursNotFoundException(etudiant.ParcoursSuivi.ToString());
        
        var parcours = parcoursList[0];

        // On vérifie si la liste des UEs du parcours contient l'UE ciblée
        bool ueAppartientAuParcours = parcours.UesEnseignees != null && parcours.UesEnseignees.Exists(u => u.Id.Equals(note.UeId));
        if (!ueAppartientAuParcours)
        {
            throw new UeNotInParcoursException($"Impossible d'ajouter la note : l'UE ID {note.UeId} ne fait pas partie du parcours ID {parcours.Id} de l'étudiant.");
        }
    }
}
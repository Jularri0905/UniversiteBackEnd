using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task AffecterNoteAsync(long idEtudiant, long idUe, float valeur);
    Task AffecterNoteAsync(Etudiant etudiant, Ue ue, float valeur);
    Task<Note?> FindNoteAsync(long idEtudiant, long idUe);
}
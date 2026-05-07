namespace DMFS.Models;

public class BreedReport
{
    public int Brid { get; set; }
    public DateTime HeatDate { get; set; }
    public DateTime BreedDate { get; set; }
    public int CowId { get; set; }
    public string CowName { get; set; } = string.Empty;
    public DateTime PregDate { get; set; }
    public DateTime ExpDate { get; set; }
    public DateTime DateCalved { get; set; }
    public int CowAge { get; set; }
    public string Remarks { get; set; } = string.Empty;

    public Cow? Cow { get; set; }
}

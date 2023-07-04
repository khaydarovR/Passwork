namespace Passwork.Server.Application.Interfaces;

public interface ISeedingService
{
    public void DbInit(bool isSeed);
    public void DropCreateDb();
}

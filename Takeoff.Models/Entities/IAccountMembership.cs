namespace Takeoff.Data
{
    public interface IAccountMembership: IMembership
    {
        string Role { get; set; }
    }
}
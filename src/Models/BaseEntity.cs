namespace Common.Models;

public class BaseEntity : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    protected BaseEntity()
    {
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }
}

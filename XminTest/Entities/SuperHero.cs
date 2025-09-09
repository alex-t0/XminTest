using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XminTest.Entities;

[Table("SuperHero")]
public class SuperHero
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
    
    [Timestamp]
    public byte[] Timestamp { get; set; }
    
    public ulong Timestamp2 { get; set; }
}
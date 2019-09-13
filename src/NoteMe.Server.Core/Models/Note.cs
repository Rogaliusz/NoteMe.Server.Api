using System;
using NoteMe.Common.DataTypes.Enums;
using NoteMe.Server.Core.Providers;

namespace NoteMe.Server.Core.Models
{
    public class Note : IIdProvider, 
        INameProvider, 
        ICreatedAtProvider, 
        IStatusProvider
    {
        public Guid Id { get; set; }
        public StatusEnum Status { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
namespace EventApp.Data.Entities {
    
    public class EventCategoryEntity : BaseEntity {
 
        public string Name { get; set; }

        public string? Description { get; set; }

        
        public virtual ICollection<EventEntity> Events { get; set; }
    
    }

}

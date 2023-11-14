using HUBShop.Enums;
using HUBShop.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUBShop.Models.Task
{
    public class Goal
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Display(Name = "Приоритет")]
        public Priority Priority { get; set; } // Навигационное свойство к Priority
        public int? PriorityValue { get; set; }
        [Display(Name = "Статус")]
        public string Status { get; set; }
        public int? StatusValue { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Дата создания")]
        public DateTime? Created { get; set; }
        [Display(Name = "Дата начала")]
        public DateTime? Started { get; set; }
        [Display(Name = "Дата Окончания")]
        public DateTime? Ended { get; set; }
        // Внешний ключ для создателя
        public int CreatorId { get; set; }
        public User? Creator { get; set; } // Навигационное свойство к создателю
        public int? ExecutorId { get; set; }
        public User? Executor { get; set; } // Навигационное свойство к исполнителю
        public Goal()
        {
            Created = DateTime.Now;
            Status = "Новая";
        }
    }
}


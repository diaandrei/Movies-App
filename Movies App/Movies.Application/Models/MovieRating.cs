﻿using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Models
{
    public class MovieRating
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public decimal Rating { get; set; }
        public string UserId { get; set; }
        public bool IsUserRated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ApplicationUser User { get; set; }
        public Movie Movie { get; set; }
    }
}

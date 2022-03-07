using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MindYourInjections.Models
{
    public class Objective
    {
        [Key]
        public int ObjectiveId {get;set;}
        [Required]
        public string Name {get;set;}
        [Required]
        public int LanguageId {get;set;}
        public Language Language {get;set;}
        public List<Step> Steps {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}
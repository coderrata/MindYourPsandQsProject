using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MindYourInjections.Models
{
    public class Language
    {
        [Key]
        public int LanguageId {get;set;}
        [Required]
        public string Name {get;set;}
        [Required]
        public int UserId {get;set;}
        public User User {get;set;}
        public List<Objective> Objectives {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}
﻿using System.ComponentModel.DataAnnotations;


namespace Inz_Fn.Models
{
    public class EditUserViewModel
    {
        //To avoid NullReferenceExceptions at runtime,
        //initialise Claims and Roles with a new list in the constructor.
        public EditUserViewModel()
        {
            Roles = new List<string>();
        }

        [Required]
        public string Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public bool EmailConfirmed { get; set; }

        public IList<string> Roles { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace RahyabIdentity.Configuration
{
    public class FaIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordRequiresDigit(){
            return new IdentityError{Code = nameof(PasswordRequiresDigit),Description = "در پسورد شما باید عدد وجود داشته باشد"};
        }
        public override IdentityError PasswordRequiresLower(){
            return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "در پسورد شما باید حروف کوچک وجود داشته باشد" };
        }
        public override IdentityError PasswordRequiresUpper(){
            return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "در پسورد شما باید حروف بزرگ وجود داشته باشد" };
        }
        public override IdentityError PasswordTooShort(int length){
            return new IdentityError { Code = nameof(PasswordTooShort), Description = "پسورد شما کوتاه است" };
        }
        public override IdentityError PasswordRequiresNonAlphanumeric(){
            return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "در پسورد شما باید کارکتری به غیر از حرف و عدد هم وجود داشته باشد" };
        }
        public override IdentityError DuplicateEmail(string email){
            return new IdentityError { Code = nameof(DuplicateEmail), Description = "پست الکترونیکی شما تکراری است" };
        }
        public override IdentityError DuplicateUserName(string userName){
            return new IdentityError { Code = nameof(DuplicateUserName), Description = "نام کاربری شما تکراری است" };
        }
        public override IdentityError InvalidUserName(string userName){
            return new IdentityError { Code = nameof(InvalidUserName), Description = "نام کاربری شما اشتباه است" };
        }
    }
}

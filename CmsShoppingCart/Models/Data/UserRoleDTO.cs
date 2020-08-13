using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CmsShoppingCart.Models.Data
{
    [Table("tblUserRoles")]
    public class UserRoleDTO
    {
        //these explanations exisit on my (mosh) paper
        //this still normal coloumn unless you don't write UserDTO and [ForeignKey(UserId)] "Code IN Bottom"

        // Order meaning : in natural way primary key comes first then other coloumns come after,
        // so now if you have two primary keys you must select and write the order what it comes first.
        [Key,Column(Order=0)]
        public int UserId { get; set; }//-----------------------------------------------------//
                                                                                              //
        //this still normal coloumn unless you don't                                          //
        //write RoleDTO and [ForeignKey(RoleId)] "Code IN Bottom"                             //
        [Key,Column(Order =1)]                                                                //
        public int RoleId { get; set; }//-----------------------------------------------------//-----------//
                                                                                              //           //
        //you must say this to make compiler know that                                        //           //
        //UserID above is a foreign key come from UserDTO Table not a normal coloumn          //           //
        [ForeignKey("UserId")]                                                                //           //
        public virtual UserDTO User { get; set; }//-------------------------------------------//           //
                                                                                                           //
        //you must say this to make compiler know that                                                     //
        //UserID above is a foreign key come from UserDTO Table not a normal coloumn                       //
        [ForeignKey("RoleId")]                                                                             //
        public virtual RoleDTO Role { get; set; }//--------------------------------------------------------//


    }
}
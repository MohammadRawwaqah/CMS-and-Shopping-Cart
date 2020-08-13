# CMS and Shopping Cart
## Description
We believe that shopping should be easier and faster than before, So for that this web application comes to solve this issue.

This CMS was built with Asp.Net MVC 5 and Entity FrameWork 6.

This CMS has three types of roles:
- Admin: Who's has full authority for (CRUD) operations for (pages, images, categories, products, texts). He can see all orders done on this system with full information related to.

- User: Who can browse categories and products with prices and galary images and put what he want to buy in his cart. User can see all his orders done on this cms with all information related to that order.

- Guest user: Who can just browse categories and products with prices and galary images.

## Tools, Libraries, APIs, Technologies Used:

-  PayPal Api to accept credit card.
 

- Unreal SMTP Server, Every order done, Admin will recieve a notification with all this order info(quantity, general price, time, name of client, and so on).

- Client can send emails to admin by filling data in Email & message Form in Contact Us section. Admin will recieve these emails to same SMTP Server.
 

- Libraries:

  - CKEditor with Roxy fileman, To give admin ability to organize content.

  - DropzoneJS to drag and drop for gallery images. 

  - FancyBox, for displaying images with zooming.

  - ToastrJS, for simple toast notification.

  - Font awesome site for icons like (home, admin Area, etc).

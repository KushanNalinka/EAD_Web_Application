using Microsoft.AspNetCore.Mvc;
using EADWebApplication.Models;
using EADWebApplication.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EADWebApplication.Helpers;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly VendorService _vendorService;
        private readonly UserService _userService;
        private readonly JwtHelper _jwtHelper;

        public VendorController(VendorService vendorService, JwtHelper jwtHelper, UserService userService)
        {
            _vendorService = vendorService;
            _jwtHelper = jwtHelper;
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]  // Only Admins can create vendors
        [HttpPost("create")]
        public async Task<IActionResult> CreateVendor([FromBody] VendorCreateModel vendorModel)
        {
            // Check if the email exists in both Vendor and User collections
            if (await _vendorService.GetVendorByEmailAsync(vendorModel.Email) != null || await _userService.GetUserByEmailAsync(vendorModel.Email) != null)
            {
                return BadRequest("Vendor with this email already exists.");
            }

            var newVendor = new Vendor
            {
                VendorName = vendorModel.VendorName,
                Email = vendorModel.Email,
                Password = UserService.EncryptPassword(vendorModel.Password),  // Encrypt password
                Category = vendorModel.Category
            };

            await _vendorService.CreateVendorAsync(newVendor);
            return Ok("Vendor created successfully.");
        }





        [Authorize(Roles = "Admin")]
        [HttpPut("update/{vendorId}")]
        public async Task<IActionResult> UpdateVendor(string vendorId, [FromBody] VendorCreateModel updatedVendor)
        {
            var existingVendor = await _vendorService.GetVendorByEmailAsync(updatedVendor.Email);
            if (existingVendor == null)
            {
                return NotFound("Vendor not found.");
            }

            // Update fields
            existingVendor.VendorName = updatedVendor.VendorName;
            existingVendor.Email = updatedVendor.Email;
            existingVendor.Password = UserService.EncryptPassword(updatedVendor.Password);
            existingVendor.Category = updatedVendor.Category;

            // Sync the changes to the User collection
            await _vendorService.UpdateVendorAsync(existingVendor);
            return Ok("Vendor updated successfully.");
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{email}")]
        public async Task<IActionResult> DeleteVendor(string email)
        {
            await _vendorService.DeleteVendorAsync(email);
            return Ok("Vendor deleted successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("activate/{email}")]
        public async Task<IActionResult> ActivateVendor(string email)
        {
            try
            {
                await _vendorService.ActivateVendorAsync(email);
                return Ok("Vendor and associated user activated successfully.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("deactivate/{email}")]
        public async Task<IActionResult> DeactivateVendor(string email)
        {
            try
            {
                await _vendorService.DeactivateVendorAsync(email);
                return Ok("Vendor and associated user deactivated successfully.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }



        // List all vendors with comments and ranks
        [HttpGet("list")]
        public async Task<IActionResult> ListVendors()
        {
            var vendors = await _vendorService.GetVendorsAsync();
            return Ok(vendors);
        }
        //Get a Single Vendor
       
        [HttpGet("{vendorId}")]
        public async Task<IActionResult> GetVendorById(string vendorId)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null)
            {
                return NotFound("Vendor not found.");
            }

            return Ok(vendor);
        }


        [Authorize]
        [HttpPost("comment/{vendorId}")]
        public async Task<IActionResult> AddComment(string vendorId, [FromBody] CommentModel model)
        {
            var userId = User.FindFirst("UserId")?.Value;  // Get User ID from token
            await _vendorService.AddCommentAsync(vendorId, model.Comment, model.Rank, userId);
            return Ok("Comment added successfully.");
        }

        [Authorize]
        [HttpPut("comment/{vendorId}/{commentId}")]
        public async Task<IActionResult> UpdateComment(string vendorId, string commentId, [FromBody] CommentModel model)
        {
            var userId = User.FindFirst("UserId")?.Value;  // Get User ID from token
            await _vendorService.UpdateCommentAsync(vendorId, commentId, model.Comment, model.Rank, userId);
            return Ok("Comment updated successfully.");
        }

        [Authorize]
        [HttpDelete("comment/{vendorId}/{commentId}")]
        public async Task<IActionResult> DeleteComment(string vendorId, string commentId)
        {
            var userId = User.FindFirst("UserId")?.Value;  // Get User ID from token
            await _vendorService.DeleteCommentAsync(vendorId, commentId, userId);
            return Ok("Comment deleted successfully.");
        }

        [Authorize]
        [HttpGet("user-comments")]
        public async Task<IActionResult> GetUserCommentsOnVendors()
        {
            // Extract the userId from the token
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            // Fetch all vendors and filter comments made by the user
            var vendors = await _vendorService.GetVendorsAsync();
            var userComments = vendors
                .Where(v => v.Comments.Any(c => c.UserId == userId))  // Find vendors with user comments
                .Select(v => new
                {
                    VendorId = v.Id,
                    VendorName = v.VendorName,
                    VendorEmail = v.Email,
                    Comments = v.Comments
                        .Where(c => c.UserId == userId)  // Get only the comments made by the logged-in user
                        .Select(c => new
                        {
                            CommentId = c.Id,
                            Comment = c.Comment,
                            Rank = c.Rank
                        })
                }).ToList();

            // Return the user comments
            return Ok(userComments);
        }

        [Authorize]
        [HttpGet("my-comments")]
        public async Task<IActionResult> GetUserComments()
        {
            var userId = User.FindFirst("UserId")?.Value;  // Get User ID from the token

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in the token.");
            }

            // Retrieve vendors where the current user has added comments
            var vendorsWithUserComments = await _vendorService.GetVendorsWithUserCommentsAsync(userId);

            // Transform the data to match the required structure
            var response = vendorsWithUserComments.SelectMany(vendor => vendor.Comments
                .Where(comment => comment.UserId == userId)
                .Select(comment => new
                {
                    vendorId = vendor.Id,
                    vendorName = vendor.VendorName,
                    vendorEmail = vendor.Email,
                    commentId = comment.Id,
                    comment = comment.Comment,
                    rank = comment.Rank
                })).ToList();

            return Ok(response);
        }





    }

    public class CommentModel
    {
        public string Comment { get; set; }
        public int Rank { get; set; }
    }

    
}

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helper;
using DatingApp.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        private readonly IOptions<CloudinarySettings> _cloudinaryconfig;

         Cloudinary cloudinary; 
        public PhotosController(IDatingRepository repo, IMapper mapper,
                                    IOptions<CloudinarySettings> cloudinaryconfig)
        {
            this._cloudinaryconfig = cloudinaryconfig;
            this._mapper = mapper;
            this._repo = repo;

            Account account = new Account(
                _cloudinaryconfig.Value.CloudName,
                _cloudinaryconfig.Value.ApiKey,
                _cloudinaryconfig.Value.ApiSecret
            );

            cloudinary = new Cloudinary(account);
           
        }

        [HttpGet("{id}", Name ="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photofromrepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotosForReturnDto>(photofromrepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotosForCreationDto dtos)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
              return Unauthorized();

            var userFromrepo = await _repo.GetUser(userId);

            var file = dtos.File;

            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = cloudinary.Upload(uploadParams);
                }
            }

            dtos.Url = uploadResult.Uri.ToString();
            dtos.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(dtos);

            if(!userFromrepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            userFromrepo.Photos.Add(photo);

            if(await _repo.SaveAll())
            {
                //return Ok();
                var photoToReturn = _mapper.Map<PhotosForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id}, photoToReturn);
            }
                
            return BadRequest("Could not add the photo");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int Id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
              return Unauthorized();

            var user = await _repo.GetUser(userId);

            if(!user.Photos.Any(p=>p.Id == Id))
                return Unauthorized();

            var photofromrepo = await _repo.GetPhoto(Id);

            if(photofromrepo.IsMain)    
                return BadRequest("You cannot delete your main photo");


            if(photofromrepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photofromrepo.PublicId);

                var result = cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                {
                    _repo.Delete(photofromrepo);
                }
            }

            if(photofromrepo.PublicId == null)
            {
                 _repo.Delete(photofromrepo);
            }

            if(await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }


        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int Id)
        {
             if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
              return Unauthorized();

            var user = await _repo.GetUser(userId);

            if(!user.Photos.Any(p=>p.Id == Id))
                return Unauthorized();


            var photofromrepo = await _repo.GetPhoto(Id);

            if(photofromrepo.IsMain)    
                return BadRequest("This is already a Main Photo");
            
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photofromrepo.IsMain = true;

            if(await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");
        }
    }
}
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace CompanyApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class CloudinaryUploadExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ImageUploadResult UploadImageCompany(this IFormFile file)
        {
            Account account = new Account();
            Cloudinary cloudinary = new Cloudinary(account);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Transformation = new Transformation().Crop("limit").Width(200).Height(200)
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            return uploadResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ImageUploadResult UploadAvatarLead(this IFormFile file)
        {
            Account account = new Account();
            Cloudinary cloudinary = new Cloudinary(account);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Transformation = new Transformation().Crop("limit").Width(400).Height(400)
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            return uploadResult;
        }
    }
}

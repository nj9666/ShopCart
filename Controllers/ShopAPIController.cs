using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopCart.Models;

using System.Net.Http.Handlers;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using ShopCart.Models.VModels;
using System.Net.Http.Headers;
using MimeKit;
using MailKit.Net.Smtp;

namespace ShopCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopAPIController : ControllerBase
    {

        private const string DEFAULT_KEY = "#kl?~@<z";

        private DummyShopContext _dbContext;


        public string TempPath = Path.Combine("public", "temp");
        public string ImgPath = Path.Combine("public", "subproduct");

        HttpCommonResponse objHttpCommonResponse = new HttpCommonResponse();

        public ShopAPIController(DummyShopContext context)
        {
            _dbContext = context;
        }


        private static Random random = new Random();


        [HttpPost]
        [Route("Upload_Image")]
        public IActionResult Upload_Image()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            string tkn = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            List<string> imgary = new List<string>();
            InputData objInput = new InputData();
            try
            {
                //foreach (var file in Request.Form.Files)
                //{
                //    var folderName = Path.Combine("public", "temp");
                //    var imgname = "img_" + tkn + DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
                //    var fileName = imgname + "." + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Split(".")[1];

                //    InputData resp = uploadfile(file, folderName, fileName, "Image");
                //    if (resp.success.Value)
                //    {
                //        imgary.Add(resp.Data);
                //    }
                //}
                var file = Request.Form.Files.First();
                var folderName = Path.Combine("public", "temp");
                var imgname = "img_" + tkn + DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
                var fileName = imgname + "." + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Split(".")[1];

                InputData resp = uploadfile(file, folderName, fileName, "Image");
                if (resp.success.Value)
                {
                    imgary.Add(resp.Data);
                    objInput.success = true;
                    objInput.message = "Successfully";
                    objInput.Data = string.Join(",", imgary.ToArray());
                }
                else
                {
                    objInput = resp;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = string.Join(",", imgary.ToArray());
            }

            return StatusCode(200, objInput);
        }
        public InputData uploadfile(dynamic file, string folderName, string fileName, string UploadType)
        {
            InputData objInput = new InputData();
            try
            {
                bool validformate = false;

                if (UploadType == "Image")
                {
                    if (fileName.ToLower().Contains(".jpg") || fileName.ToLower().Contains(".jpeg") || fileName.ToLower().Contains(".png"))
                    {
                        validformate = true;
                    }
                }
                else if (UploadType == "Video")
                {

                    if (fileName.ToLower().Contains(".mp4") || fileName.ToLower().Contains(".avi") || fileName.ToLower().Contains(".mkv"))
                    {
                        validformate = true;
                    }
                }

                if (validformate)
                {
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    if (file.Length > 0)
                    {
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var dbPath = Path.Combine(folderName, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        objInput.success = true;
                        objInput.message = "Successfully";
                        objInput.Data = "temp/" + fileName;
                    }
                    else
                    {
                        objInput.success = false;
                        objInput.message = "Problem during file uploading";
                        objInput.Data = null;
                    }
                }
                else
                {
                    objInput.success = false;
                    objInput.message = "Invalid formate file..";
                    objInput.Data = fileName;
                }
            }
            catch (Exception ex)
            {
                objInput.success = false;

                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            return objInput;
        }



        #region User Area


        [HttpPost]
        [Route("User/Registration")]
        public dynamic User_Registration([FromBody]UserMstr objUserData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.UserMstr.Where(p => p.ContactNumber == objUserData.ContactNumber || p.Email == objUserData.Email).Any())
                {
                    objInput.success = false;
                    objInput.message = "Phone Number or Email already register!!";
                    objInput.Data = null;
                }
                else
                {
                    //UserMstr newUser = new UserMstr();
                    //newUser.UserName = objUserData.UserName;
                    //newUser.FirstName = objUserData.FirstName;
                    //newUser.LastName = objUserData.LastName;
                    //newUser.Gender = objUserData.Gender;
                    //newUser.Dob = objUserData.Dob;
                    //newUser.ContactNumber = objUserData.ContactNumber;
                    //newUser.Email = objUserData.Email;
                    //newUser.Password = objUserData.Password;


                    int otp = new Random().Next(100000, 999999);
                    objUserData.CurrentOtp = otp.ToString();
                    objUserData.OtpExTime = DateTime.UtcNow.AddMinutes(10);
                    objUserData.IsActive = true;
                    objUserData.IsDeleted = false;
                    objUserData.CreateDt = DateTime.UtcNow;
                    objUserData.UpdateDt = DateTime.UtcNow;

                    DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    String xToken = objUserData.Id.ToString() + "#" + objUserData.UserName + "#" + tomorrow.ToString();
                    objUserData.Token = objInput.AuthToken = Encrypt(xToken);

                    _dbContext.UserMstr.Add(objUserData);
                    _dbContext.SaveChanges();

                    objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Registration Successful.";
                    objInput.Data = objUserData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("User/Forget_pwd")]
        public dynamic User_Forget_pwd([FromBody]UserMstr objUserData)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.UserMstr.Where(p => p.Email == objUserData.Email).Any())
                {
                    objInput.success = false;
                    objInput.message = "This Email is not register!!";
                    objInput.Data = null;
                }
                else
                {
                    UserMstr user =  _dbContext.UserMstr.Where(p => p.Email == objUserData.Email).FirstOrDefault();
                    var mail_html = "<!DOCTYPE html><html><head><title></title><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /><meta name='viewport' content='width=device-width, initial-scale=1'><meta http-equiv='X-UA-Compatible' content='IE=edge' /><style type='text/css'>@media screen{@font-face{font-family:'Lato';font-style:normal;font-weight:400;src:local('Lato Regular'), local('Lato-Regular'), url(https://fonts.gstatic.com/s/lato/v11/qIIYRU-oROkIk8vfvxw6QvesZW2xOQ-xsNqO47m55DA.woff) format('woff')}@font-face{font-family:'Lato';font-style:normal;font-weight:700;src:local('Lato Bold'), local('Lato-Bold'), url(https://fonts.gstatic.com/s/lato/v11/qdgUG4U09HnJwhYI-uK18wLUuEpTyoUstqEm5AMlJo4.woff) format('woff')}@font-face{font-family:'Lato';font-style:italic;font-weight:400;src:local('Lato Italic'), local('Lato-Italic'), url(https://fonts.gstatic.com/s/lato/v11/RYyZNoeFgb0l7W3Vu1aSWOvvDin1pK8aKteLpeZ5c0A.woff) format('woff')}@font-face{font-family:'Lato';font-style:italic;font-weight:700;src:local('Lato Bold Italic'), local('Lato-BoldItalic'), url(https://fonts.gstatic.com/s/lato/v11/HkF_qI1x_noxlxhrhMQYELO3LdcAZYWl9Si6vvxL-qU.woff) format('woff')}}body,table,td,a{-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%}table,td{mso-table-lspace:0pt;mso-table-rspace:0pt}img{-ms-interpolation-mode:bicubic}img{border:0;height:auto;line-height:100%;outline:none;text-decoration:none}table{border-collapse:collapse !important}body{height:100% !important;margin:0 !important;padding:0 !important;width:100% !important}a[x-apple-data-detectors]{color:inherit !important;text-decoration:none !important;font-size:inherit !important;font-family:inherit !important;font-weight:inherit !important;line-height:inherit !important}@media screen and (max-width:600px){h1{font-size:32px !important;line-height:32px !important}}div[style*='margin: 16px 0;']{margin:0 !important}</style></head><body style='background-color: #f4f4f4; margin: 0 !important; padding: 0 !important;'><div style='display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: Lato, Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;'> We weren't able to process your latest payment. Mind checking things out?</div><table border='0' cellpadding='0' cellspacing='0' width='100%'><tr><td bgcolor='#ff857a' align='center'><table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'><tr><td align='center' valign='top' style='padding: 40px 10px 40px 10px;'> <img alt='Logo' src='https://i.ibb.co/n3YRSnw/logo.png' width='auto' style='display: block;width: 100%;max-width: 250px;min-width: 40px;font-family: Lato, Helvetica, Arial, sans-serif;color: #ffffff;font-size: 18px;' border='0'></td></tr></table></td></tr><tr><td bgcolor='#ff857a' align='center' style='padding: 0px 10px 0px 10px;'><table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'><tr><td bgcolor='#ffffff' align='center' valign='top' style='padding: 40px 20px 20px 20px; border-radius: 4px 4px 0px 0px; color: #111111; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 48px; font-weight: 400; letter-spacing: 4px; line-height: 48px;'><h1 style='font-size: 48px; font-weight: 400; margin: 0;'>Get your Password!</h1></td></tr></table></td></tr><tr><td bgcolor='#f4f4f4' align='center' style='padding: 0px 10px 0px 10px;'><table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'><tr><td bgcolor='#ffffff' align='left' style='padding: 20px 30px 40px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'><p style='margin: 0;'>This forget password request is sent by your account if the sender is you and everything is good then go and login in Doorja.</p></td></tr><tr><td bgcolor='#fff1ec' align='left' style='padding: 24px 48px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px; border-radius: 4px; '><p> To keep your account secure, you need to update your password. Use the login credentials to login into Doorja as usual.</p><p style='color: #ff857a;''><b>Your Email : " + user.Email + "<br> Your password: " + user.Password + "</b></p> <a href=' https://localhost:44363/Login/' style='color: #ff857a; text-decoration: underline;'>Login now</a><p> If you think this email was sent in error, please contact our team by phone at +1 (800) 123-4567 or reply to this email directly. Thank you!</p></td></tr><tr><td bgcolor='#ffffff' align='left' style='padding: 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'><p style='margin: 0;'>If you have any questions, just reply to this email—we're always happy to help out.</p></td></tr><tr><td bgcolor='#ffffff' align='left' style='padding: 0px 30px 40px 30px; border-radius: 0px 0px 4px 4px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'><p style='margin: 0;'>Cheers,<br>The Doorja Team</p></td></tr></table></td></tr><tr><td bgcolor='#f4f4f4' align='center' style='padding: 30px 10px 0px 10px;'><table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'><tr><td bgcolor='#fff1ec' align='center' style='padding: 30px 30px 30px 30px; border-radius: 4px 4px 4px 4px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'><h2 style='font-size: 20px; font-weight: 400; color: #111111; margin: 0;'>Need more help?</h2><p style='margin: 0;'><a href='javascript:void(0)' style='color: #ff857a;'>We&rsquo;re here, ready to talk</a></p></td></tr></table></td></tr><tr><td bgcolor='#f4f4f4' align='center' style='padding: 0px 10px 0px 10px;'><table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'><tr><td bgcolor='#f4f4f4' align='left' style='padding: 30px 30px 30px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 14px; font-weight: 400; line-height: 18px;'><p style='margin: 0;'> <a href='javascript:void(0)' style='color: #111111; font-weight: 700;'>Dashboard</a> - <a href='javascript:void(0)' style='color: #111111; font-weight: 700;'>Billing</a> - <a href='javascript:void(0)' style='color: #111111; font-weight: 700;'>Help</a></p></td></tr><tr><td bgcolor='#f4f4f4' align='left' style='padding: 0px 30px 30px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 14px; font-weight: 400; line-height: 18px;'><p style='margin: 0;'>You received this email because we had problems billing your account. If it looks weird, <a href='javascript:void(0)' style='color: #111111; font-weight: 700;'>view it in your browser</a>.</p></td></tr><tr><td bgcolor='#f4f4f4' align='left' style='padding: 0px 30px 30px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 14px; font-weight: 400; line-height: 18px;'><p style='margin: 0;'>If these emails get annoying, please feel free to <a href='javascript:void(0)' style='color: #111111; font-weight: 700;'>unsubscribe</a>.</p></td></tr><tr><td bgcolor='#f4f4f4' align='left' style='padding: 0px 30px 30px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 14px; font-weight: 400; line-height: 18px;'><p style='margin: 0;'>Ceej - 1234 Main Street - Anywhere, MA - 56789</p></td></tr></table></td></tr></table></body></html>";

                    sendMail(user.Email, "Your Doorja login credentials", mail_html);


                    objInput.success = true;
                    objInput.message = "We sent you email regarding your login credentials on this mail address \" "+ objUserData.Email+" \".";
                    objInput.Data = objUserData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("User/Login")]
        public dynamic User_Login([FromBody]UserMstr objUserData)
        {
            InputData objInput = new InputData();
            try
            {
                List<UserMstr> Data = new List<UserMstr>();
                if (decimal.TryParse(objUserData.Email, out _))
                {
                    Data = _dbContext.UserMstr.Where(p => (p.ContactNumber == Decimal.Parse(objUserData.Email)) && p.Password == objUserData.Password).ToList();
                }
                else
                {
                    Data = _dbContext.UserMstr.Where(p => (p.Email == objUserData.Email) && p.Password == objUserData.Password).ToList();
                }


                if (Data.Count == 0)
                {
                    objInput.success = false;
                    objInput.message = "Incorrect Username or Password.";
                    objInput.Data = null;
                }
                else
                {
                    if (Data[0].IsActive)
                    {
                        Data[0].Password = null;
                        DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                        string xToken = Data[0].Id.ToString() + "#" + Data[0].UserName + "#" + tomorrow.ToString();

                        objInput.success = true;
                        objInput.message = "Successfully.";
                        objInput.Data = Data[0];
                        objInput.AuthToken = Encrypt(xToken);
                    }
                    else
                    {
                        objInput.success = false;
                        objInput.message = "This user is currently deactivate.";
                        objInput.Data = null;
                    }
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("User/Edit/{UserId}")]
        public dynamic User_Edit([FromBody]UserMstr objUserData, int UserId)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                if (!_dbContext.UserMstr.Where(p => p.Id == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "User is Not Exist!!";
                    objInput.Data = null;
                }
                else if (_dbContext.UserMstr.Where(p => p.ContactNumber == objUserData.ContactNumber && p.Id != UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Phone Number already register!!";
                    objInput.Data = null;
                }
                else
                {
                    UserMstr dbobjUser = _dbContext.UserMstr.Find(UserId);
                    dbobjUser.UserName = objUserData.UserName == null ? dbobjUser.UserName : objUserData.UserName;
                    dbobjUser.FirstName = objUserData.FirstName == null ? dbobjUser.FirstName : objUserData.FirstName;
                    dbobjUser.LastName = objUserData.LastName == null ? dbobjUser.LastName : objUserData.LastName;
                    dbobjUser.Gender = objUserData.Gender;
                    dbobjUser.Dob = objUserData.Dob == null ? dbobjUser.Dob : objUserData.Dob;
                    dbobjUser.ContactNumber = objUserData.ContactNumber == 0 ? dbobjUser.ContactNumber : objUserData.ContactNumber;
                    dbobjUser.Email = objUserData.Email == null ? dbobjUser.Email : objUserData.Email;
                    dbobjUser.UpdateDt = DateTime.UtcNow;

                    _dbContext.SaveChanges();

                    DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    String xToken = objUserData.Id.ToString() + "#" + objUserData.UserName + "#" + tomorrow.ToString();
                    objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbobjUser;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("User/Changepwd")]
        public dynamic User_Changepwd([FromBody]changePass objpass)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();
                if (!_dbContext.UserMstr.Where(p => p.Id == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "User is Not Exist!!";
                    objInput.Data = null;
                }
                else if (!_dbContext.UserMstr.Where(p => p.Id == UserId && p.Password == objpass.oldpass).Any())
                {
                    objInput.success = false;
                    objInput.message = "your Current Password is not match !!";
                    objInput.Data = null;
                }
                else
                {
                    UserMstr dbobjUser = _dbContext.UserMstr.Where(p => p.Id == UserId && p.Password == objpass.oldpass).FirstOrDefault();

                    if (dbobjUser != null)
                    {
                        dbobjUser.Password = objpass.newpass;
                        dbobjUser.UpdateDt = DateTime.UtcNow;
                        _dbContext.SaveChanges();
                    }

                    objInput.success = true;
                    objInput.message = "Your password is change successfully. ";
                    objInput.Data = dbobjUser;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("User/GetAll")]
        public dynamic User_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.UserMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "User is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<UserMstr> dbUser = _dbContext.UserMstr.ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbUser;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("User/Get")]
        public dynamic User_Get()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();
                if (_dbContext.UserMstr.Find(UserId) == null)
                {
                    objInput.success = false;
                    objInput.message = "User is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    UserMstr dbUser = _dbContext.UserMstr.Find(UserId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbUser;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("User/Layout/CategoryAll")]
        public dynamic User_Layout_menu()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CategoryMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Category is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<CategoryMstr> dbCategory = _dbContext.CategoryMstr.OrderBy(p => p.PCatId).ToList();
                    //List<VMCategoryMstr> vMCategories = new List<VMCategoryMstr>();
                    //foreach (CategoryMstr cat in dbCategory)
                    //{
                    //    VMCategoryMstr vMCategory = new VMCategoryMstr();
                    //    vMCategory.Id = cat.Id;
                    //    vMCategory.PCatId = cat.PCatId;
                    //    vMCategory.Name = cat.Name;
                    //    vMCategories.Add(vMCategory);
                    //}
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCategory;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }



        [HttpGet]
        [Route("User/Home/TopColl")]
        public dynamic User_TopColl()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.ProductMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Product is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<ProductMstr> products = _dbContext.ProductMstr.Where(p => p.SubProductTbl.Count > 0 && p.UserListing).OrderByDescending(p => p.CurrentRating).Take(8).ToList();



                    if (products.Count > 0)
                    {
                        foreach (ProductMstr pro in products)
                        {
                            pro.SubProductTbl = _dbContext.SubProductTbl.Where(p => p.ProductId == pro.Id).ToList();
                            foreach (SubProductTbl subPro in pro.SubProductTbl)
                            {
                                subPro.Color = _dbContext.ColoursTbl.Find(subPro.ColorId);
                                subPro.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == subPro.Id).ToList();
                            }
                        }
                        objInput.success = true;
                        objInput.message = "Successfully. ";
                        objInput.Data = products;
                    }
                    else
                    {
                        objInput.success = false;
                        objInput.message = "No Product avalable. ";
                        objInput.Data = null;
                    }
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("User/Home/TodayDeal")]
        public dynamic User_TodayDeal()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.ProductMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Product is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<TodayDealsTbl> todayDeals = _dbContext.TodayDealsTbl.Where(p => p.StartDate.Date == DateTime.Today.Date).ToList();

                    //List<TodayDealsTbl> todayDeals = _dbContext.TodayDealsTbl.Where(p => p.StartDate.Date == DateTime.Parse("2020-06-30 00:00:00.000").Date).ToList();
                    List<ProductMstr> dealproducts = new List<ProductMstr>();
                    foreach (TodayDealsTbl td in todayDeals)
                    {
                        ProductMstr pm = _dbContext.ProductMstr.Where(p => p.Id == td.ProId && p.SubProductTbl.Count > 0 && p.UserListing).FirstOrDefault();
                        if (pm != null)
                        {
                            dealproducts.Add(pm);
                        }
                    }



                    if (dealproducts.Count > 0)
                    {
                        foreach (ProductMstr pro in dealproducts)
                        {
                            pro.SubProductTbl = _dbContext.SubProductTbl.Where(p => p.ProductId == pro.Id).ToList();
                            foreach (SubProductTbl subPro in pro.SubProductTbl)
                            {
                                subPro.Color = _dbContext.ColoursTbl.Find(subPro.ColorId);
                                subPro.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == subPro.Id).ToList();
                            }
                        }


                        objInput.success = true;
                        objInput.message = "Successfully. ";
                        objInput.Data = dealproducts;
                    }
                    else
                    {
                        objInput.success = false;
                        objInput.message = "No Product avalable. ";
                        objInput.Data = null;
                    }
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }



        [HttpGet]
        [Route("User/Home/NewProduct")]
        public dynamic User_NewProduct()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.ProductMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Product is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<ProductMstr> dealproducts = _dbContext.ProductMstr.Where(p => p.SubProductTbl.Count > 0 && p.UserListing).OrderByDescending(p => p.CreateDt).Take(8).ToList();
                    //List<ProductMstr> dealproducts = new List<ProductMstr>();



                    if (dealproducts.Count > 0)
                    {
                        foreach (ProductMstr pro in dealproducts)
                        {
                            pro.SubProductTbl = _dbContext.SubProductTbl.Where(p => p.ProductId == pro.Id).ToList();
                            foreach (SubProductTbl subPro in pro.SubProductTbl)
                            {
                                subPro.Color = _dbContext.ColoursTbl.Find(subPro.ColorId);
                                subPro.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == subPro.Id).ToList();
                            }
                        }


                        objInput.success = true;
                        objInput.message = "Successfully. ";
                        objInput.Data = dealproducts;
                    }
                    else
                    {
                        objInput.success = false;
                        objInput.message = "No Product avalable. ";
                        objInput.Data = null;
                    }
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }



        [HttpGet]
        [Route("User/Home/BestSellProduct")]
        public dynamic User_BestSellProduct()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.ProductMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Product is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<ProductMstr> dealproducts = new List<ProductMstr>();
                    //List<ProductMstr> dealproducts = _dbContext.ProductMstr.Where(p => p.SubProductTbl.Count > 0 && p.UserListing).OrderByDescending(p => p.CreateDt).Take(8).ToList();

                    List<orderByP> orderByPs = new List<orderByP>();
                    var Order_ByProduct = _dbContext.OrderDetailsTbl.OrderBy(p => p.SubProducatId).GroupBy(p => p.SubProducatId);


                    foreach (var gp in Order_ByProduct)
                    {
                        orderByP orderByP = new orderByP();
                        foreach (OrderDetailsTbl item in gp)
                        {
                            var orderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.Id == item.Id).Select(ss => new
                            {
                                pic = ss.SubProducat.ProductImg.FirstOrDefault().Path,
                                name = ss.SubProducat.Product.Name,
                                sku = ss.SubProducat.Product.Sku,
                                price = ss.SubProducat.Price,
                                color = ss.SubProducat.Color.Name,
                                size = ss.SubProducat.Size.Name,
                            }).FirstOrDefault();

                            orderByP.pic = orderDetailsTbl.pic;
                            orderByP.name = orderDetailsTbl.name;
                            orderByP.sku = orderDetailsTbl.sku;
                            orderByP.price = orderDetailsTbl.price;
                            orderByP.color = orderDetailsTbl.color;
                            orderByP.size = orderDetailsTbl.size;

                            orderByP.totalCount += 1;
                        }
                        orderByP.totalAmount = (orderByP.totalCount * orderByP.price);
                        orderByPs.Add(orderByP);
                    }

                    var obpro = orderByPs.OrderByDescending(p => p.totalCount).GroupBy(p => p.sku).Take(8).ToList();


                    foreach (var obp in obpro)
                    {
                        ProductMstr pro = _dbContext.ProductMstr.Where(p => p.Sku == obp.Key).Where(p => p.SubProductTbl.Count > 0 && p.UserListing).FirstOrDefault();
                        if (pro != null)
                        {
                            dealproducts.Add(pro);
                        }
                    }


                    if (dealproducts.Count > 0)
                    {
                        foreach (ProductMstr pro in dealproducts)
                        {
                            pro.SubProductTbl = _dbContext.SubProductTbl.Where(p => p.ProductId == pro.Id).ToList();
                            foreach (SubProductTbl subPro in pro.SubProductTbl)
                            {
                                subPro.Color = _dbContext.ColoursTbl.Find(subPro.ColorId);
                                subPro.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == subPro.Id).ToList();
                            }
                        }


                        objInput.success = true;
                        objInput.message = "Successfully. ";
                        objInput.Data = dealproducts;
                    }
                    else
                    {
                        objInput.success = false;
                        objInput.message = "No Product avalable. ";
                        objInput.Data = null;
                    }
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("User/Category/{catid}")]
        public dynamic User_Category_product(int catid)
        {
            InputData objInput = new InputData();
            try
            {
                List<ProductMstr> products = new List<ProductMstr>();
                if (_dbContext.CategoryMstr.Find(catid).PCatId == null)
                {
                    products = _dbContext.ProductMstr.Where(p => p.SubProductTbl.Count > 0 && p.UserListing && (p.CatId == catid)).OrderByDescending(p => p.CurrentRating).ToList();
                    List<CategoryMstr> categories = _dbContext.CategoryMstr.Where(p => p.PCatId == catid).ToList();
                    foreach (CategoryMstr cat in categories)
                    {
                        products.AddRange(_dbContext.ProductMstr.Where(p => p.SubProductTbl.Count > 0 && p.UserListing && p.CatId == cat.Id).OrderByDescending(p => p.CurrentRating).ToList());
                    }
                }
                else
                {
                    products = _dbContext.ProductMstr.Where(p => p.SubProductTbl.Count > 0 && p.UserListing && (p.CatId == catid)).OrderByDescending(p => p.CurrentRating).ToList();
                }



                if (products.Count > 0)
                {
                    foreach (ProductMstr pro in products)
                    {
                        pro.SubProductTbl = _dbContext.SubProductTbl.Where(p => p.ProductId == pro.Id).ToList();
                        foreach (SubProductTbl subPro in pro.SubProductTbl)
                        {
                            subPro.Color = _dbContext.ColoursTbl.Find(subPro.ColorId);
                            subPro.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == subPro.Id).ToList();
                        }
                    }
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = products;
                }
                else
                {
                    objInput.success = false;
                    objInput.message = "No Product avalable. ";
                    objInput.Data = null;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }





        #region User Activity Area

        //[HttpPost]
        //[Route("Card/Insert")]
        //public dynamic Card_Insert([FromBody]CardsTbl objCardsData)
        //{
        //    InputData objInput = new InputData();
        //    try
        //    {
        //        InputData objInputdd = new InputData();
        //        if (!ValidateToken(ref objInputdd))
        //        {
        //            return Unauthorized();
        //        }
        //        if (_dbContext.CardsTbl.Where(p => p.CardNumber == objCardsData.CardNumber && p.UserId == objCardsData.UserId).Any())
        //        {
        //            objInput.success = false;
        //            objInput.message = "Cards is already register!!";
        //            objInput.Data = null;
        //        }
        //        else
        //        {

        //            objCardsData.IsActive = true;
        //            objCardsData.IsDeleted = false;
        //            objCardsData.CreateDt = DateTime.UtcNow;
        //            objCardsData.UpdateDt = DateTime.UtcNow;
        //            _dbContext.CardsTbl.Add(objCardsData);
        //            _dbContext.SaveChanges();

        //            //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
        //            //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
        //            //objInput.AuthToken = Encrypt(xToken);
        //            objInput.success = true;
        //            objInput.message = "Successfully. ";
        //            objInput.Data = objCardsData;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        objInput.success = false;
        //        objInput.message = "Something went wrong, please contact support";
        //        objInput.Data = null;
        //    }

        //    objHttpCommonResponse.success = objInput.success;
        //    objHttpCommonResponse.message = objInput.message;
        //    objHttpCommonResponse.data = objInput.Data;
        //    //objHttpCommonResponse.AuthToken = objInput.AuthToken;
        //    return Ok(objHttpCommonResponse);
        //}

        //[HttpPost]
        //[Route("Card/Edit/{CardId}")]
        //public dynamic Card_Edit([FromBody]CardsTbl objCardsData, int CardId)
        //{
        //    InputData objInput = new InputData();
        //    try
        //    {
        //        if (_dbContext.CardsTbl.Where(p => p.Id == CardId).Any())
        //        {
        //            objInput.success = false;
        //            objInput.message = "Cards is ont Exist!!";
        //            objInput.Data = null;
        //        }
        //        else if (_dbContext.CardsTbl.Where(p => p.CardNumber == objCardsData.CardNumber && p.UserId == objCardsData.UserId).Any())
        //        {
        //            objInput.success = false;
        //            objInput.message = "Cards is already register!!";
        //            objInput.Data = null;
        //        }
        //        else
        //        {
        //            CardsTbl dbobjCards = _dbContext.CardsTbl.Find(CardId);
        //            dbobjCards.CardNumber = objCardsData.CardNumber == 0 ? dbobjCards.CardNumber : objCardsData.CardNumber;
        //            dbobjCards.ExMonth = objCardsData.ExMonth == 0 ? dbobjCards.ExMonth : objCardsData.ExMonth;
        //            dbobjCards.ExYear = objCardsData.ExYear == 0 ? dbobjCards.ExYear : objCardsData.ExYear;
        //            dbobjCards.HolderName = objCardsData.HolderName == null ? dbobjCards.HolderName : objCardsData.HolderName;
        //            dbobjCards.CardLabel = objCardsData.CardLabel == null ? dbobjCards.CardLabel : objCardsData.CardLabel;
        //            dbobjCards.UpdateDt = DateTime.UtcNow;
        //            _dbContext.SaveChanges();

        //            //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
        //            //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
        //            //objInput.AuthToken = Encrypt(xToken);
        //            objInput.success = true;
        //            objInput.message = "Successfully. ";
        //            objInput.Data = dbobjCards;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        objInput.success = false;
        //        objInput.message = "Something went wrong, please contact support";
        //        objInput.Data = null;
        //    }

        //    objHttpCommonResponse.success = objInput.success;
        //    objHttpCommonResponse.message = objInput.message;
        //    objHttpCommonResponse.data = objInput.Data;
        //    //objHttpCommonResponse.AuthToken = objInput.AuthToken;
        //    return Ok(objHttpCommonResponse);
        //}

        //[HttpPost]
        //[Route("Card/Remove/{CardId}")]
        //public dynamic Card_Remove(int CardId)
        //{
        //    InputData objInput = new InputData();
        //    try
        //    {
        //        if (_dbContext.CardsTbl.Where(p => p.Id == CardId).Any())
        //        {
        //            objInput.success = false;
        //            objInput.message = "Cards is ont Exist!!";
        //            objInput.Data = null;
        //        }

        //        else
        //        {
        //            CardsTbl objCardsData = _dbContext.CardsTbl.Find(CardId);
        //            objCardsData.IsActive = false;
        //            objCardsData.IsDeleted = true;
        //            objCardsData.UpdateDt = DateTime.UtcNow;
        //            _dbContext.SaveChanges();

        //            //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
        //            //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
        //            //objInput.AuthToken = Encrypt(xToken);
        //            objInput.success = true;
        //            objInput.message = "Successfully. ";
        //            objInput.Data = objCardsData;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        objInput.success = false;
        //        objInput.message = "Something went wrong, please contact support";
        //        objInput.Data = null;
        //    }

        //    objHttpCommonResponse.success = objInput.success;
        //    objHttpCommonResponse.message = objInput.message;
        //    objHttpCommonResponse.data = objInput.Data;
        //    //objHttpCommonResponse.AuthToken = objInput.AuthToken;
        //    return Ok(objHttpCommonResponse);
        //}

        //[HttpGet]
        //[Route("Card/Get/{CardId}")]
        //public dynamic Card_Get(int CardId)
        //{
        //    InputData objInput = new InputData();
        //    try
        //    {
        //        if (_dbContext.CardsTbl.Where(p => p.Id == CardId).Any())
        //        {
        //            objInput.success = false;
        //            objInput.message = "Cards is ont Exist!!";
        //            objInput.Data = null;
        //        }

        //        else
        //        {
        //            CardsTbl objCardsData = _dbContext.CardsTbl.Find(CardId);

        //            //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
        //            //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
        //            //objInput.AuthToken = Encrypt(xToken);
        //            objInput.success = true;
        //            objInput.message = "Successfully. ";
        //            objInput.Data = objCardsData;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        objInput.success = false;
        //        objInput.message = "Something went wrong, please contact support";
        //        objInput.Data = null;
        //    }

        //    objHttpCommonResponse.success = objInput.success;
        //    objHttpCommonResponse.message = objInput.message;
        //    objHttpCommonResponse.data = objInput.Data;
        //    //objHttpCommonResponse.AuthToken = objInput.AuthToken;
        //    return Ok(objHttpCommonResponse);
        //}

        //[HttpGet]
        //[Route("Card/GetAll/My")]
        //public dynamic Card_GetAll()
        //{
        //    InputData objInput = new InputData();
        //    try
        //    {
        //        int UserId = 2;
        //        if (!_dbContext.CardsTbl.Where(p => p.UserId == UserId).Any())
        //        {
        //            objInput.success = false;
        //            objInput.message = "Cards is ont Exist!!";
        //            objInput.Data = null;
        //        }

        //        else
        //        {

        //            List<CardsTbl> objCardsData = _dbContext.CardsTbl.Where(p => p.UserId == UserId).ToList();

        //            //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
        //            //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
        //            //objInput.AuthToken = Encrypt(xToken);
        //            objInput.success = true;
        //            objInput.message = "Successfully. ";
        //            objInput.Data = objCardsData;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        objInput.success = false;
        //        objInput.message = "Something went wrong, please contact support";
        //        objInput.Data = null;
        //    }

        //    objHttpCommonResponse.success = objInput.success;
        //    objHttpCommonResponse.message = objInput.message;
        //    objHttpCommonResponse.data = objInput.Data;
        //    //objHttpCommonResponse.AuthToken = objInput.AuthToken;
        //    return Ok(objHttpCommonResponse);
        //}


        //[HttpGet]
        //[Route("Card/Get/Default")]
        //public dynamic Card_Get()
        //{
        //    InputData objInput = new InputData();
        //    try
        //    {
        //        int UserId = 2;
        //        if (!_dbContext.CardsTbl.Where(p => p.UserId == UserId && p.IsDefault == true).Any())
        //        {
        //            objInput.success = false;
        //            objInput.message = "Cards is ont Exist!!";
        //            objInput.Data = null;
        //        }
        //        else
        //        {
        //            CardsTbl objCardsData = _dbContext.CardsTbl.Where(p => p.UserId == UserId && p.IsDefault == true).FirstOrDefault();

        //            //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
        //            //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
        //            //objInput.AuthToken = Encrypt(xToken);
        //            objInput.success = true;
        //            objInput.message = "Successfully. ";
        //            objInput.Data = objCardsData;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        objInput.success = false;
        //        objInput.message = "Something went wrong, please contact support";
        //        objInput.Data = null;
        //    }

        //    objHttpCommonResponse.success = objInput.success;
        //    objHttpCommonResponse.message = objInput.message;
        //    objHttpCommonResponse.data = objInput.Data;
        //    //objHttpCommonResponse.AuthToken = objInput.AuthToken;
        //    return Ok(objHttpCommonResponse);
        //}


        [HttpPost]
        [Route("Address/Insert")]
        public dynamic Address_Insert([FromBody]AddressTbl objAddresssData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();
                if (false)
                {
                    objInput.success = false;
                    objInput.message = "Addresss is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    if(!_dbContext.AddressTbl.Where(p => p.UserId == UserId).Any())
                    {
                        objAddresssData.IsDefault = true;
                    }
                    objAddresssData.UserId = UserId;
                    objAddresssData.IsActive = true;
                    objAddresssData.IsDeleted = false;
                    objAddresssData.CreateDt = DateTime.UtcNow;
                    objAddresssData.UpdateDt = DateTime.UtcNow;
                    _dbContext.AddressTbl.Add(objAddresssData);
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Adress added Successfully. ";
                    objInput.Data = objAddresssData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Address/Edit/{AddressId}")]
        public dynamic Address_Edit([FromBody]AddressTbl objAddresssData, int AddressId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.AddressTbl.Where(p => p.Id == AddressId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Addresss is ont Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    AddressTbl dbobjAddresss = _dbContext.AddressTbl.Find(AddressId);
                    dbobjAddresss.Line1 = objAddresssData.Line1 == null ? dbobjAddresss.Line1 : objAddresssData.Line1;
                    dbobjAddresss.Line2 = objAddresssData.Line2 == null ? dbobjAddresss.Line2 : objAddresssData.Line2;
                    dbobjAddresss.CityId = objAddresssData.CityId == 0 ? dbobjAddresss.CityId : objAddresssData.CityId;
                    dbobjAddresss.StateId = objAddresssData.StateId == 0 ? dbobjAddresss.StateId : objAddresssData.StateId;
                    dbobjAddresss.CountryId = objAddresssData.CountryId == 0 ? dbobjAddresss.CountryId : objAddresssData.CountryId;
                    dbobjAddresss.Landmark = objAddresssData.Landmark == null ? dbobjAddresss.Landmark : objAddresssData.Landmark;
                    dbobjAddresss.Zip = objAddresssData.Zip == 0 ? dbobjAddresss.Zip : objAddresssData.Zip;
                    dbobjAddresss.Type = objAddresssData.Type == 0 ? dbobjAddresss.Type : objAddresssData.Type;
                    dbobjAddresss.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbobjAddresss;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Address/Remove/{AddressId}")]
        public dynamic Address_Remove(int AddressId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.AddressTbl.Where(p => p.Id == AddressId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Addresss is ont Exist!!";
                    objInput.Data = null;
                }

                else
                {
                    AddressTbl objAddresssData = _dbContext.AddressTbl.Find(AddressId);
                    objAddresssData.IsActive = false;
                    objAddresssData.IsDeleted = true;
                    objAddresssData.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Removed Successfully.";
                    objInput.Data = objAddresssData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Address/Get/{AddressId}")]
        public dynamic Address_Get(int AddressId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.AddressTbl.Where(p => p.Id == AddressId && p.IsDeleted == false).Any())
                {
                    objInput.success = false;
                    objInput.message = "Addresss is ont Exist!!";
                    objInput.Data = null;
                }

                else
                {
                    AddressTbl objAddresssData = _dbContext.AddressTbl.Where(p => p.Id == AddressId && p.IsDeleted == false).FirstOrDefault();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objAddresssData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Address/GetAll/My")]
        public dynamic Address_GetAll()
        {
            InputData objInput = new InputData();
            try
            {

                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();
                if (!_dbContext.AddressTbl.Where(p => p.UserId == UserId && p.IsDeleted == false).Any())
                {
                    objInput.success = false;
                    objInput.message = "Addresss is ont Exist!!";
                    objInput.Data = null;
                }

                else
                {

                    List<AddressTbl> objAddresssData = _dbContext.AddressTbl.Where(p => p.UserId == UserId && p.IsDeleted == false).ToList();
                    foreach (AddressTbl add in objAddresssData)
                    {
                        add.City = _dbContext.CityMstr.Find(add.CityId);
                        add.State = _dbContext.StateMstr.Find(add.StateId);
                        add.Country = _dbContext.CountryMstr.Find(add.CountryId);
                    }
                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objAddresssData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Address/Get/Default")]
        public dynamic Address_Get()
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = 2;
                if (!_dbContext.AddressTbl.Where(p => p.UserId == UserId && p.IsDefault == true).Any())
                {
                    objInput.success = false;
                    objInput.message = "Addresss is ont Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    AddressTbl objAddresssData = _dbContext.AddressTbl.Where(p => p.UserId == UserId && p.IsDefault == true && p.IsDeleted == false).FirstOrDefault();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objAddresssData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Address/Set/Default/{AddressId}")]
        public dynamic Address_Set_D(int AddressId)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (!_dbContext.AddressTbl.Where(p => p.Id == AddressId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Addresss is ont Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    AddressTbl updatedadd = null;
                    List<AddressTbl> objAddresssList = _dbContext.AddressTbl.Where(p => p.UserId == GetAuthId()).ToList();
                    foreach (AddressTbl add in objAddresssList)
                    {
                        if (add.Id == AddressId)
                        {
                            add.IsDefault = true;
                            updatedadd = add;
                        }
                        else
                        {
                            add.IsDefault = false;
                        }

                    }

                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = updatedadd;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Cart/Insert/{SubProducatId}")]
        public dynamic Cart_Insert(int SubProducatId)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();
                if (!_dbContext.UserMstr.Where(p => p.Id == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (_dbContext.CartTbl.Where(p => p.SubProducatId == SubProducatId && p.UserId == UserId).Any())
                {
                    CartTbl objCartData = _dbContext.CartTbl.Single(p => p.SubProducatId == SubProducatId && p.UserId == UserId);
                    objCartData.Qty = objCartData.Qty + 1;

                    _dbContext.SaveChanges();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCartData;
                }
                else
                {
                    CartTbl objCartData = new CartTbl();
                    objCartData.UserId = UserId;
                    objCartData.SubProducatId = SubProducatId;
                    objCartData.Qty = 1;


                    objCartData.IsActive = true;
                    objCartData.IsDeleted = false;
                    objCartData.CreateDt = DateTime.UtcNow;
                    objCartData.UpdateDt = DateTime.UtcNow;
                    _dbContext.CartTbl.Add(objCartData);
                    _dbContext.SaveChanges();


                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAddresssData.Id.ToString() + "#" + objAddresssData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCartData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Cart/Qty/{CartId}/{Qty}")]
        public dynamic Cart_Change_Qty(int CartId, int Qty)
        {
            InputData objInput = new InputData();
            try
            {

                int UserId = GetAuthId();
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (!_dbContext.CartTbl.Where(p => p.Id == CartId && p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cart Item is not Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    CartTbl objCartData = _dbContext.CartTbl.Find(CartId);
                    objCartData.Qty = Qty;
                    objCartData.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCartData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Cart/Remove/{CartId}")]
        public dynamic Cart_Remove(int CartId)
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = GetAuthId();
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (!_dbContext.CartTbl.Where(p => p.Id == CartId && p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cart Item is not Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    CartTbl objCartData = _dbContext.CartTbl.Find(CartId);
                    _dbContext.CartTbl.Remove(objCartData);
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCartData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Cart/GetAll")]
        public dynamic Cart_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = GetAuthId();
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (!_dbContext.CartTbl.Where(p => p.UserId == UserId).Any())
                {
                    objInput.success = true;
                    objInput.message = "Cart List is Empty!!";
                    objInput.Data = null;
                }
                else
                {

                    List<CartTbl> carts = _dbContext.CartTbl.Where(p => p.UserId == UserId).ToList();
                    foreach (CartTbl cart in carts)
                    {
                        SubProductTbl sp = _dbContext.SubProductTbl.Find(cart.SubProducatId);
                        sp.Product = _dbContext.ProductMstr.Find(sp.ProductId);
                        sp.Color = _dbContext.ColoursTbl.Find(sp.ColorId);
                        sp.Size = _dbContext.SizesTbl.Find(sp.SizeId);
                        sp.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == sp.Id).ToList();

                        TodayDealsTbl dealsTbl = _dbContext.TodayDealsTbl.Where(p => p.ProId == sp.ProductId && p.StartDate.Date == DateTime.Today.Date).FirstOrDefault();
                        if (dealsTbl != null)
                        {
                            if (dealsTbl.DiscountType == 1)
                            {
                                sp.Price -= dealsTbl.DiscountAmount;
                            }
                            else if (dealsTbl.DiscountType == 2)
                            {
                                var discount = (sp.Price * dealsTbl.DiscountAmount) / 100;
                                sp.Price -= discount;
                            }
                        }
                        cart.SubProducat = sp;
                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = carts;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Wish/Insert")]
        public dynamic Wish_Insert([FromBody]WishlistTbl objWishData)
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = GetAuthId();
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (_dbContext.WishlistTbl.Where(p => p.SubProducatId == objWishData.SubProducatId && p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Alraedy Exist in Wish List!!";
                    objInput.Data = null;
                }
                else
                {
                    objWishData.IsActive = true;
                    objWishData.IsDeleted = false;
                    objWishData.CreateDt = DateTime.UtcNow;
                    objWishData.UpdateDt = DateTime.UtcNow;
                    _dbContext.WishlistTbl.Add(objWishData);
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objWishData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Wish/Remove/{WishId}")]
        public dynamic Wish_Remove(int WishId)
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = GetAuthId();
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (_dbContext.WishlistTbl.Where(p => p.Id == WishId && p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cart Item is not Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    WishlistTbl objWishData = _dbContext.WishlistTbl.Find(WishId);
                    _dbContext.WishlistTbl.Remove(objWishData);
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objWishData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Wish/GetAll")]
        public dynamic Wish_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = GetAuthId();
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (!_dbContext.WishlistTbl.Where(p => p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Wish List is Empty!!";
                    objInput.Data = null;
                }
                else
                {

                    List<WishlistTbl> wishlists = _dbContext.WishlistTbl.Where(p => p.UserId == UserId).ToList();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = wishlists;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }



        [HttpPost]
        [Route("Order/Place/{AddressId}/{CpCode}")]
        public dynamic Order_Place(int AddressId, string CpCode)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                int UserId = GetAuthId();

                OrderTbl NewOrder = new OrderTbl();
                NewOrder.OrderIdV = "OD"+DateTime.UtcNow.ToString("yyyyMMddHHmmssffff").ToString();
                NewOrder.UserId = UserId;
                decimal discountAmount = 0;

                List<CartTbl> items = _dbContext.CartTbl.Where(p => p.UserId == UserId).ToList();
                foreach (CartTbl item in items)
                {
                    NewOrder.TotalQty += item.Qty;
                    SubProductTbl sp = _dbContext.SubProductTbl.Find(item.SubProducatId);
                    sp.Qty -= item.Qty;
                    NewOrder.TotalPrice += (sp.Price * item.Qty);
                    _dbContext.CartTbl.Remove(item);
                }
                NewOrder.AddressId = AddressId;
                CouponMstr activeCoupon = _dbContext.CouponMstr.Where(p => p.CoupCode == CpCode).FirstOrDefault();
                if (activeCoupon == null)
                {
                    NewOrder.CouponCode = 0;
                }
                else
                {
                    if (activeCoupon.DiscountType == 1)
                    {
                        discountAmount = Convert.ToDecimal(activeCoupon.DiscountAmount);
                        NewOrder.TotalPrice -= discountAmount;
                    }
                    else if (activeCoupon.DiscountType == 2)
                    {
                        discountAmount = (NewOrder.TotalPrice * Convert.ToDecimal(activeCoupon.DiscountAmount)) / 100;
                        NewOrder.TotalPrice -= discountAmount;
                    }
                    NewOrder.CouponCode = activeCoupon.Id;
                }
                NewOrder.Status = 1;

                NewOrder.IsActive = true;
                NewOrder.IsDeleted = false;
                NewOrder.CreateDt = DateTime.UtcNow;
                NewOrder.UpdateDt = DateTime.UtcNow;
                _dbContext.OrderTbl.Add(NewOrder);

                _dbContext.SaveChanges();

                foreach (CartTbl item in items)
                {

                    OrderDetailsTbl ODT = new OrderDetailsTbl();
                    ODT.OrderId = NewOrder.Id;
                    ODT.SubProducatId = item.SubProducatId;
                    ODT.Qty = item.Qty;
                    ODT.OrderSubStatus = 1;
                    ODT.VenderPaymantStatus = 1;

                    ODT.IsActive = true;
                    ODT.IsDeleted = false;
                    ODT.CreateDt = DateTime.UtcNow;
                    ODT.UpdateDt = DateTime.UtcNow;
                    _dbContext.OrderDetailsTbl.Add(ODT);

                    _dbContext.SaveChanges();
                }



                //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                //String xToken = objOrderData.Id.ToString() + "#" + objOrderData.UserName + "#" + tomorrow.ToString();
                //objInput.AuthToken = Encrypt(xToken);
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = NewOrder;


                UserMstr loguser = _dbContext.UserMstr.Find(UserId);
                AddressTbl logadd = _dbContext.AddressTbl.Find(AddressId);
                logadd.City = _dbContext.CityMstr.Find(logadd.CityId);
                logadd.State = _dbContext.StateMstr.Find(logadd.StateId);
                logadd.Country = _dbContext.CountryMstr.Find(logadd.CountryId);

                var mailhtml = "<html><head> <style type='text/css'> .mailBody{text-align: center; margin: 0 auto; width: 650px; font-family: 'Open Sans', sans-serif; background-color: #e2e2e2; display: block;}ul{margin: 0; padding: 0;}li{display: inline-block; text-decoration: unset;}a{text-decoration: none;}p{margin: 15px 0;}h5{color: #444; text-align: left; font-weight: 400;}.text-center{text-align: center}.main-bg-light{background-color: #fafafa;}.title{color: #444444; font-size: 22px; font-weight: bold; margin-top: 10px; margin-bottom: 10px; padding-bottom: 0; text-transform: uppercase; display: inline-block; line-height: 1;}table{margin-top: 30px}table.top-0{margin-top: 0;}table.order-detail{border: 1px solid #ddd; border-collapse: collapse;}table.order-detail tr:nth-child(even){border-top: 1px solid #ddd; border-bottom: 1px solid #ddd;}table.order-detail tr:nth-child(odd){border-bottom: 1px solid #ddd;}.pad-left-right-space{border: unset !important;}.pad-left-right-space td{padding: 5px 15px;}.pad-left-right-space td p{margin: 0;}.pad-left-right-space td b{font-size: 15px; font-family: 'Roboto', sans-serif;}.order-detail th{font-size: 16px; padding: 15px; text-align: center; background: #fafafa;}.footer-social-icon tr td img{margin-left: 5px; margin-right: 5px;}</style> </head><body> <table class='mailBody'> <tr><td> <table align='center' border='0' cellpadding='0' cellspacing='0' style='padding: 0 30px;background-color: #fff; -webkit-box-shadow: 0px 0px 14px -4px rgba(0, 0, 0, 0.2705882353);box-shadow: 0px 0px 14px -4px rgba(0, 0, 0, 0.2705882353);width: 100%;'> <tbody> <tr> <td> <table align='left' border='0' cellpadding='0' cellspacing='0' style='text-align: left;' width='100%'> <tr> <td style='text-align: center;'> <img src='https://i.ibb.co/pyw5g1S/delivery.png' alt='delivery' style='margin-bottom: 30px;'> </td></tr><tr> <td> <p style='font-size: 14px;'><b>Hi " + loguser.FirstName + " " + loguser.LastName + ",</b></p><p style='font-size: 14px;'>Order Is Successfully Processsed And Your Order Is On The Way,</p><p style='font-size: 14px;'>Order ID : " + NewOrder.OrderIdV + ",</p></td></tr></table> <table cellpadding='0' cellspacing='0' border='0' align='left' style='width: 100%;margin-top: 10px; margin-bottom: 10px;'> <tbody> <tr> <td style='background-color: #fafafa;border: 1px solid #ddd;padding: 15px;letter-spacing: 0.3px;width: 50%;'> <h5 style='font-size: 16px; font-weight: 600;color: #000; line-height: 16px; padding-bottom: 13px; border-bottom: 1px solid #e6e8eb; letter-spacing: -0.65px; margin-top:0; margin-bottom: 13px;'> Seller Address</h5> <p style='text-align: left;font-weight: normal; font-size: 14px; color: #000000;line-height: 21px; margin-top: 0;'> " + logadd.Line1 + ",<br>" + logadd.Line2 + "<br>" + logadd.Landmark + ", " + logadd.City.Name + " " + logadd.State.Name + " " + logadd.Country.Name + " - " + logadd.Zip + " </p></td></tr></tbody> </table> <table class='order-detail' border='0' cellpadding='0' cellspacing='0' align='left' style='width: 100%; margin-bottom: 50px;'> <tr align='left'> <th>PRODUCT</th> <th style='padding-left: 15px;'>DESCRIPTION</th> <th>QUANTITY</th> <th>PRICE </th> </tr>";
                foreach (CartTbl Cartitem in items)
                {

                    SubProductTbl sp = _dbContext.SubProductTbl.Find(Cartitem.SubProducatId);
                    ProductMstr pro = _dbContext.ProductMstr.Find(sp.ProductId);
                    sp.Color = _dbContext.ColoursTbl.Find(sp.ColorId);
                    sp.Size = _dbContext.SizesTbl.Find(sp.SizeId);
                    var spprice = decimal.Round(sp.Price, 2, MidpointRounding.AwayFromZero);

                    mailhtml += "<tr> <td> <img src='https://i.ibb.co/2NJJdzx/18.jpg' alt='18'  width='80'> </td><td valign='top' style='padding-left: 15px;'> <h5 style='margin-top: 15px;'>" + pro.Name + " </h5> </td><td valign='top' style='padding-left: 15px;'> <h5 style='font-size: 14px; color:#444;margin-top:15px; margin-bottom: 0px;'>Size : <span> " + sp.Size.Name + "</span> </h5> <h5 style='font-size: 14px; color:#444;margin-top: 10px;'>Colour : <span style='height:20px;width:20px;border:1px solid #444;    display: inline-block;vertical-align: middle;background-color:" + sp.Color.Name + "'></span></h5> <h5 style='font-size: 14px; color:#444;margin-top: 10px;'>QTY : <span>" + Cartitem.Qty + "</span></h5> </td><td valign='top' style='padding-left: 15px;'> <h5 style='font-size: 14px; color:#444;margin-top:15px'><b>" + spprice + "</b></h5> </td></tr>";
                }

                mailhtml += "<tr class='pad-left-right-space '> <td class='m-t-5' colspan='2' align='left'> <p style='font-size: 14px;'>Subtotal : </p></td><td class='m-t-5' colspan='2' align='right'> <b style>" + (NewOrder.TotalPrice + discountAmount) + "</b> </td></tr>" +

                    "<tr class='pad-left-right-space'> <td colspan='2' align='left'> <p style='font-size: 14px;'>Shipping Charge :</p></td><td colspan='2' align='right'> <b>100</b> </td></tr><tr class='pad-left-right-space'> <td colspan='2' align='left'> <p style='font-size: 14px;'>Discount :</p></td><td colspan='2' align='right'> <b> "+ discountAmount + "</b> </td></tr><tr class='pad-left-right-space '> <td class='m-b-5' colspan='2' align='left'> <p style='font-size: 14px;'>Total :</p></td><td class='m-b-5' colspan='2' align='right'> <b>" + (NewOrder.TotalPrice + 100) + "</b> </td></tr></table> </td></tr></tbody> </table> <table class='main-bg-light text-center top-0' align='center' border='0' cellpadding='0' cellspacing='0' width='100%'><tbody> <tr> <td style='padding: 30px;'> <table width='100%'><tr><td> <h4 class='title' style='margin:0;text-align: center;'>Follow us</h4> </td></tr></table><table border='0' cellpadding='0' cellspacing='0' class='footer-social-icon' align='center' class='text-center' style='margin-top:20px;'> <tr> <td> <a href='#'><img src='https://i.ibb.co/QjvxQ9W/facebook.png' alt='facebook'></a> </td><td> <a href='#'><img src='https://i.ibb.co/99xXqMn/youtube.png' alt='youtube'></a> </td><td> <a href='#'><img src='https://i.ibb.co/Z6v2wRZ/twitter.png' alt='twitter'></a> </td><td> <a href='#'><img src='https://i.ibb.co/2K26WcD/gplus.png' alt='gplus'></a> </td><td> <a href='#'><img src='https://i.ibb.co/tMp1rcZ/linkedin.png' alt='linkedin'></a> </td><td> <a href='#'><img src='https://i.ibb.co/GvJtjd0/pinterest.png' alt='pinterest'></a> </td></tr></table> <table style='border-top: 1px solid #ddd; margin: 20px auto 0;'><tr><td></td></tr></table><table border='0' cellpadding='0' cellspacing='0' width='100%' style='margin: 20px auto 0;'> <tr> <td> <a href='#' style='font-size:13px'>Want to change how you receive these emails?</a> </td></tr><tr> <td> <p style='font-size:13px; margin:0;'>2018 - 19 Copy Right by Doorja. powerd by Doorja ShopCart LTD.</p></td></tr><tr> <td> <a href='#' style='font-size:13px; margin:0;text-decoration: underline;'>Unsubscribe</a> </td></tr></table> </td></tr></tbody></table></td></tr></table></body></html>";
                sendMail(loguser.Email, "Your Order Successfully Placed", mailhtml);
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Order/Place/{AddressId}")]
        public dynamic Order_Place(int AddressId)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();

                OrderTbl NewOrder = new OrderTbl();
                NewOrder.OrderIdV = "OD" + DateTime.UtcNow.ToString("yyyyMMddHHmmssffff").ToString();
                NewOrder.UserId = UserId;

                List<CartTbl> items = _dbContext.CartTbl.Where(p => p.UserId == UserId).ToList();
                foreach (CartTbl item in items)
                {
                    NewOrder.TotalQty += item.Qty;
                    SubProductTbl sp = _dbContext.SubProductTbl.Find(item.SubProducatId);
                    sp.Qty -= item.Qty;
                    NewOrder.TotalPrice += (sp.Price * item.Qty);

                    _dbContext.CartTbl.Remove(item);
                }
                NewOrder.AddressId = AddressId;
                NewOrder.CouponCode = null;
                NewOrder.Status = 0;

                NewOrder.IsActive = true;
                NewOrder.IsDeleted = false;
                NewOrder.CreateDt = DateTime.UtcNow;
                NewOrder.UpdateDt = DateTime.UtcNow;
                _dbContext.OrderTbl.Add(NewOrder);

                _dbContext.SaveChanges();

                foreach (CartTbl item in items)
                {

                    OrderDetailsTbl ODT = new OrderDetailsTbl();
                    ODT.OrderId = NewOrder.Id;
                    ODT.SubProducatId = item.SubProducatId;
                    ODT.Qty = item.Qty;
                    ODT.OrderSubStatus = 0;
                    ODT.VenderPaymantStatus = 0;

                    ODT.IsActive = true;
                    ODT.IsDeleted = false;
                    ODT.CreateDt = DateTime.UtcNow;
                    ODT.UpdateDt = DateTime.UtcNow;
                    _dbContext.OrderDetailsTbl.Add(ODT);

                    _dbContext.SaveChanges();
                }



                //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                //String xToken = objOrderData.Id.ToString() + "#" + objOrderData.UserName + "#" + tomorrow.ToString();
                //objInput.AuthToken = Encrypt(xToken);
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = NewOrder;

                UserMstr loguser = _dbContext.UserMstr.Find(UserId);
                AddressTbl logadd = _dbContext.AddressTbl.Find(AddressId);
                logadd.City = _dbContext.CityMstr.Find(logadd.CityId);
                logadd.State = _dbContext.StateMstr.Find(logadd.StateId);
                logadd.Country = _dbContext.CountryMstr.Find(logadd.CountryId);

                var mailhtml = "<html><head> <style type='text/css'> .mailBody{text-align: center; margin: 0 auto; width: 650px; font-family: 'Open Sans', sans-serif; background-color: #e2e2e2; display: block;}ul{margin: 0; padding: 0;}li{display: inline-block; text-decoration: unset;}a{text-decoration: none;}p{margin: 15px 0;}h5{color: #444; text-align: left; font-weight: 400;}.text-center{text-align: center}.main-bg-light{background-color: #fafafa;}.title{color: #444444; font-size: 22px; font-weight: bold; margin-top: 10px; margin-bottom: 10px; padding-bottom: 0; text-transform: uppercase; display: inline-block; line-height: 1;}table{margin-top: 30px}table.top-0{margin-top: 0;}table.order-detail{border: 1px solid #ddd; border-collapse: collapse;}table.order-detail tr:nth-child(even){border-top: 1px solid #ddd; border-bottom: 1px solid #ddd;}table.order-detail tr:nth-child(odd){border-bottom: 1px solid #ddd;}.pad-left-right-space{border: unset !important;}.pad-left-right-space td{padding: 5px 15px;}.pad-left-right-space td p{margin: 0;}.pad-left-right-space td b{font-size: 15px; font-family: 'Roboto', sans-serif;}.order-detail th{font-size: 16px; padding: 15px; text-align: center; background: #fafafa;}.footer-social-icon tr td img{margin-left: 5px; margin-right: 5px;}</style> </head><body> <table class='mailBody'> <tr><td> <table align='center' border='0' cellpadding='0' cellspacing='0' style='padding: 0 30px;background-color: #fff; -webkit-box-shadow: 0px 0px 14px -4px rgba(0, 0, 0, 0.2705882353);box-shadow: 0px 0px 14px -4px rgba(0, 0, 0, 0.2705882353);width: 100%;'> <tbody> <tr> <td> <table align='left' border='0' cellpadding='0' cellspacing='0' style='text-align: left;' width='100%'> <tr> <td style='text-align: center;'> <img src='https://i.ibb.co/pyw5g1S/delivery.png' alt='delivery' style='margin-bottom: 30px;'> </td></tr><tr> <td> <p style='font-size: 14px;'><b>Hi " + loguser.FirstName + " " + loguser.LastName + ",</b></p><p style='font-size: 14px;'>Order Is Successfully Processsed And Your Order Is On The Way,</p><p style='font-size: 14px;'>Order ID : " + NewOrder.OrderIdV + ",</p></td></tr></table> <table cellpadding='0' cellspacing='0' border='0' align='left' style='width: 100%;margin-top: 10px; margin-bottom: 10px;'> <tbody> <tr> <td style='background-color: #fafafa;border: 1px solid #ddd;padding: 15px;letter-spacing: 0.3px;width: 50%;'> <h5 style='font-size: 16px; font-weight: 600;color: #000; line-height: 16px; padding-bottom: 13px; border-bottom: 1px solid #e6e8eb; letter-spacing: -0.65px; margin-top:0; margin-bottom: 13px;'> Seller Address</h5> <p style='text-align: left;font-weight: normal; font-size: 14px; color: #000000;line-height: 21px; margin-top: 0;'> " + logadd.Line1 + ",<br>" + logadd.Line2 + "<br>" + logadd.Landmark + ", " + logadd.City.Name + " " + logadd.State.Name + " " + logadd.Country.Name + " - " + logadd.Zip + " </p></td></tr></tbody> </table> <table class='order-detail' border='0' cellpadding='0' cellspacing='0' align='left' style='width: 100%; margin-bottom: 50px;'> <tr align='left'> <th>PRODUCT</th> <th style='padding-left: 15px;'>DESCRIPTION</th> <th>QUANTITY</th> <th>PRICE </th> </tr>";
                foreach (CartTbl Cartitem in items)
                {

                    SubProductTbl sp = _dbContext.SubProductTbl.Find(Cartitem.SubProducatId);
                    ProductMstr pro = _dbContext.ProductMstr.Find(sp.ProductId);
                    sp.Color = _dbContext.ColoursTbl.Find(sp.ColorId);
                    sp.Size = _dbContext.SizesTbl.Find(sp.SizeId);
                    var spprice = decimal.Round(sp.Price, 2, MidpointRounding.AwayFromZero);

                    mailhtml += "<tr> <td> <img src='https://i.ibb.co/2NJJdzx/18.jpg' alt='18'  width='80'> </td><td valign='top' style='padding-left: 15px;'> <h5 style='margin-top: 15px;'>" + pro.Name + " </h5> </td><td valign='top' style='padding-left: 15px;'> <h5 style='font-size: 14px; color:#444;margin-top:15px; margin-bottom: 0px;'>Size : <span> " + sp.Size.Name + "</span> </h5> <h5 style='font-size: 14px; color:#444;margin-top: 10px;'>Colour : <span style='height:20px;width:20px;border:1px solid #444;    display: inline-block;vertical-align: middle;background-color:" + sp.Color.Name + "'></span></h5> <h5 style='font-size: 14px; color:#444;margin-top: 10px;'>QTY : <span>" + Cartitem.Qty + "</span></h5> </td><td valign='top' style='padding-left: 15px;'> <h5 style='font-size: 14px; color:#444;margin-top:15px'><b>" + spprice + "</b></h5> </td></tr>";
                }

                mailhtml += "<tr class='pad-left-right-space '> <td class='m-t-5' colspan='2' align='left'> <p style='font-size: 14px;'>Subtotal : </p></td><td class='m-t-5' colspan='2' align='right'> <b style>" + (NewOrder.TotalPrice ) + "</b> </td></tr>" +

                    "<tr class='pad-left-right-space'> <td colspan='2' align='left'> <p style='font-size: 14px;'>Shipping Charge :</p></td><td colspan='2' align='right'> <b>100</b> </td></tr><tr class='pad-left-right-space'> <td colspan='2' align='left'> <p style='font-size: 14px;'>Discount :</p></td><td colspan='2' align='right'> <b> 0</b> </td></tr><tr class='pad-left-right-space '> <td class='m-b-5' colspan='2' align='left'> <p style='font-size: 14px;'>Total :</p></td><td class='m-b-5' colspan='2' align='right'> <b>" + (NewOrder.TotalPrice + 100) + "</b> </td></tr></table> </td></tr></tbody> </table> <table class='main-bg-light text-center top-0' align='center' border='0' cellpadding='0' cellspacing='0' width='100%'> <tbody> <tr> <td style='padding: 30px;'> <table width='100%'><tr><td> <h4 class='title' style='margin:0;text-align: center;'>Follow us</h4> </td></tr></table><table border='0' cellpadding='0' cellspacing='0' class='footer-social-icon' align='center' class='text-center' style='margin-top:20px;'> <tr> <td> <a href='#'><img src='https://i.ibb.co/QjvxQ9W/facebook.png' alt='facebook'></a> </td><td> <a href='#'><img src='https://i.ibb.co/99xXqMn/youtube.png' alt='youtube'></a> </td><td> <a href='#'><img src='https://i.ibb.co/Z6v2wRZ/twitter.png' alt='twitter'></a> </td><td> <a href='#'><img src='https://i.ibb.co/2K26WcD/gplus.png' alt='gplus'></a> </td><td> <a href='#'><img src='https://i.ibb.co/tMp1rcZ/linkedin.png' alt='linkedin'></a> </td><td> <a href='#'><img src='https://i.ibb.co/GvJtjd0/pinterest.png' alt='pinterest'></a> </td></tr></table> <table style='border-top: 1px solid #ddd; margin: 20px auto 0;'><tr><td></td></tr></table><table border='0' cellpadding='0' cellspacing='0' width='100%' style='margin: 20px auto 0;'> <tr> <td> <a href='#' style='font-size:13px'>Want to change how you receive these emails?</a> </td></tr><tr> <td> <p style='font-size:13px; margin:0;'>2018 - 19 Copy Right by Doorja. powerd by Doorja ShopCart LTD.</p></td></tr><tr> <td> <a href='#' style='font-size:13px; margin:0;text-decoration: underline;'>Unsubscribe</a> </td></tr></table> </td></tr></tbody></table></td></tr></table></body></html>";
                sendMail(loguser.Email, "Your Order Successfully Placed", mailhtml);

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }
        
        [HttpPost]
        [Route("Order/payment")]
        public dynamic Order_payment([FromBody]PaymentTbl objpaymentData)
        {
            InputData objInput = new InputData();
            try
            {
                OrderTbl order = _dbContext.OrderTbl.Find(objpaymentData.OrderId);

                byte sts = 6;
                if (objpaymentData.Status == 1)
                {
                    sts = 1;
                }

                order.Status = sts;

                foreach (OrderDetailsTbl odt in _dbContext.OrderDetailsTbl.Where(p => p.OrderId == order.Id))
                {
                    odt.OrderSubStatus = sts;
                }
                objpaymentData.IsActive = true;
                objpaymentData.IsDeleted = false;
                objpaymentData.CreateDt = DateTime.UtcNow;
                objpaymentData.UpdateDt = DateTime.UtcNow;
                _dbContext.PaymentTbl.Add(objpaymentData);

                _dbContext.SaveChanges();
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = objpaymentData;
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }
            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Order/Get/{orderid}")]
        public dynamic order_Get(string orderid)
        {
            InputData objInput = new InputData();
            try
            {
                //orderid = 30825127;
                if (!_dbContext.OrderTbl.Where(p => p.OrderIdV == orderid).Any())
                {
                    objInput.success = false;
                    objInput.message = "order List is Empty!!";
                    objInput.Data = null;
                }
                else
                {

                    OrderTbl order = _dbContext.OrderTbl.Where(p => p.OrderIdV == orderid).FirstOrDefault();
                    order.OrderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.OrderId == order.Id).ToList();
                    foreach (OrderDetailsTbl odt in order.OrderDetailsTbl)
                    {
                        SubProductTbl sp = _dbContext.SubProductTbl.Find(odt.SubProducatId);
                        sp.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == sp.Id).ToList();
                        sp.Product = _dbContext.ProductMstr.Find(sp.ProductId);

                        odt.SubProducat = sp;
                    }
                    order.User = _dbContext.UserMstr.Find(order.UserId);
                    AddressTbl add = _dbContext.AddressTbl.Find(order.AddressId);
                    add.City = _dbContext.CityMstr.Find(add.CityId);
                    add.State = _dbContext.StateMstr.Find(add.StateId);
                    add.Country = _dbContext.CountryMstr.Find(add.CountryId);
                    order.CouponCodeNavigation = _dbContext.CouponMstr.Find(order.CouponCode);
                    order.Address = add;

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = order;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Order/GetAll/ForUser")]
        public dynamic order_GetAll_User()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();

                if (!_dbContext.OrderTbl.Where(p => p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "order List is Empty!!";
                    objInput.Data = null;
                }
                else
                {

                    List<OrderTbl> Myorders = _dbContext.OrderTbl.Where(p => p.UserId == UserId).ToList();
                    List<CoutOrderTbl> MyCostOtrders = new List<CoutOrderTbl>();
                    foreach (OrderTbl order in Myorders)
                    {
                        CoutOrderTbl coutOrderTbl = new CoutOrderTbl();

                        order.OrderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.OrderId == order.Id).ToList();
                        foreach (OrderDetailsTbl odt in order.OrderDetailsTbl)
                        {
                            SubProductTbl sp = _dbContext.SubProductTbl.Find(odt.SubProducatId);
                            coutOrderTbl.ProductImg.Add(_dbContext.ProductImg.Where(p => p.SubProducatId == sp.Id).FirstOrDefault().Path);
                        }
                        order.CouponCodeNavigation = _dbContext.CouponMstr.Find(order.CouponCode);
                        coutOrderTbl.Id = order.Id;
                        coutOrderTbl.OrderIdV = order.OrderIdV;
                        coutOrderTbl.TotalQty = order.TotalQty;
                        coutOrderTbl.TotalPrice = order.TotalPrice + 100;
                        coutOrderTbl.Status = order.Status;
                        coutOrderTbl.CouponCode = order.CouponCodeNavigation?.Name;
                        coutOrderTbl.CreateDt = order.CreateDt;


                        PaymentTbl pay = _dbContext.PaymentTbl.Where(p => p.OrderId == order.Id).FirstOrDefault();
                        coutOrderTbl.PaymentMethod = pay?.Type;

                        if (pay?.Type == "CC")
                        {
                            coutOrderTbl.PaymentMethod = "Credit card";
                        }
                        else if (pay?.Type == "DC")
                        {
                            coutOrderTbl.PaymentMethod = "Debit card";
                        }
                        else if (pay?.Type == "NB")
                        {
                            coutOrderTbl.PaymentMethod = "Net banking";
                        }
                        else if (pay?.Type == "UPI")
                        {
                            coutOrderTbl.PaymentMethod = "UPI";
                        }
                        else if (pay?.Type == "PPI")
                        {
                            coutOrderTbl.PaymentMethod = "Paytm wallet";
                        }
                        else if (pay?.Type == "PAYTMCC")
                        {
                            coutOrderTbl.PaymentMethod = "Postpaid";
                        }

                        coutOrderTbl.PaymentStatus = pay != null ? pay.Status : 0;


                        MyCostOtrders.Add(coutOrderTbl);
                    }
                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = MyCostOtrders;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Order/GetForOD/{orderid}")]
        public dynamic order_GetForOD(long orderid)
        {
            InputData objInput = new InputData();
            try
            {
                //orderid = 30825127;
                if (!_dbContext.OrderTbl.Where(p => p.Id == orderid).Any())
                {
                    objInput.success = false;
                    objInput.message = "order List is Empty!!";
                    objInput.Data = null;
                }
                else
                {

                    OrderTbl order = _dbContext.OrderTbl.Where(p => p.Id == orderid).FirstOrDefault();                    
                    order.User = _dbContext.UserMstr.Find(order.UserId);                    
                    order.CouponCodeNavigation = _dbContext.CouponMstr.Find(order.CouponCode);
                    order.OrderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.OrderId == order.Id).ToList();

                    CoutOrderDetails coutOrderTbl = new CoutOrderDetails();

                    foreach (OrderDetailsTbl odt in order.OrderDetailsTbl)
                    {
                        SubProductTbl sp = _dbContext.SubProductTbl.Find(odt.SubProducatId);
                        sp.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == sp.Id).ToList();
                        sp.Color = _dbContext.ColoursTbl.Find(sp.ColorId);
                        sp.Size = _dbContext.SizesTbl.Find(sp.SizeId);
                        sp.Product = _dbContext.ProductMstr.Find(sp.ProductId);
                        sp.Product.Vender = _dbContext.VenderMstr.Find(sp.Product.VenderId);
                        odt.SubProducat = sp;
                    }
                    coutOrderTbl.MyOrders = order.OrderDetailsTbl;
                    coutOrderTbl.Id = order.Id;
                    coutOrderTbl.OrderIdV = order.OrderIdV;
                    coutOrderTbl.TotalQty = order.TotalQty;
                    coutOrderTbl.TotalPrice = order.TotalPrice + 100;
                    coutOrderTbl.Status = order.Status;
                    coutOrderTbl.CouponCode = order.CouponCodeNavigation?.Name;
                    coutOrderTbl.CreateDt = order.Status < 2 ? order.CreateDt : order.UpdateDt;

                    AddressTbl add = _dbContext.AddressTbl.Find(order.AddressId);
                    add.City = _dbContext.CityMstr.Find(add.CityId);
                    add.State = _dbContext.StateMstr.Find(add.StateId);
                    add.Country = _dbContext.CountryMstr.Find(add.CountryId);
                    coutOrderTbl.orderaddresses = add;

                    coutOrderTbl.User = _dbContext.UserMstr.Find(order.UserId);

                    PaymentTbl pay = _dbContext.PaymentTbl.Where(p => p.OrderId == order.Id).FirstOrDefault();
                    coutOrderTbl.PaymentMethod = pay?.Type;

                    if (pay?.Type == "CC")
                    {
                        coutOrderTbl.PaymentMethod = "Credit card";
                    }
                    else if (pay?.Type == "DC")
                    {
                        coutOrderTbl.PaymentMethod = "Debit card";
                    }
                    else if (pay?.Type == "NB")
                    {
                        coutOrderTbl.PaymentMethod = "Net banking";
                    }
                    else if (pay?.Type == "UPI")
                    {
                        coutOrderTbl.PaymentMethod = "UPI";
                    }
                    else if (pay?.Type == "PPI")
                    {
                        coutOrderTbl.PaymentMethod = "Paytm wallet";
                    }
                    else if (pay?.Type == "PAYTMCC")
                    {
                        coutOrderTbl.PaymentMethod = "Postpaid";
                    }

                    coutOrderTbl.PaymentStatus = pay != null ? pay.Status : 0;




                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = coutOrderTbl;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Rating/Insert")]
        public dynamic Rating_Insert([FromBody]RatingTbl objRatingData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int UserId = GetAuthId();
                objRatingData.UserId = UserId;
                if (_dbContext.RatingTbl.Where(p => p.ProductId == objRatingData.ProductId && p.UserId == objRatingData.UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Rating is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    



                    objRatingData.IsActive = true;
                    objRatingData.IsDeleted = false;
                    objRatingData.CreateDt = DateTime.UtcNow;
                    objRatingData.UpdateDt = DateTime.UtcNow;
                    _dbContext.RatingTbl.Add(objRatingData);
                    _dbContext.SaveChanges();

                    List<RatingTbl> allrating = _dbContext.RatingTbl.Where(p => p.ProductId == objRatingData.ProductId).ToList();
                    decimal totalrat = 0;
                    var totalratcount = allrating.Count;
                    foreach (RatingTbl rat in allrating)
                    {
                        totalrat += rat.Rating;
                    }
                    ProductMstr pro = _dbContext.ProductMstr.Find(objRatingData.ProductId);
                    pro.CurrentRating = totalrat / totalratcount;
                    pro.RatingCount = totalratcount;
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objRatingData.Id.ToString() + "#" + objRatingData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objRatingData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Rating/Edit")]
        public dynamic Rating_Edit([FromBody]RatingTbl objRatingData)
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = GetAuthId();
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.UserMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (!_dbContext.RatingTbl.Where(p => p.Id == objRatingData.Id && p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Rating is Not Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    RatingTbl newrating = _dbContext.RatingTbl.Find(objRatingData.Id);
                    newrating.Rating = objRatingData.Rating;
                    newrating.Review = objRatingData.Review;
                    objRatingData.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objRatingData.Id.ToString() + "#" + objRatingData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objRatingData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        #endregion

        #endregion


        #region Master Admin Area


        [HttpPost]
        [Route("Adimn/Insert")]
        public dynamic Adimn_Insert([FromBody]AdminMstr objAdminData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.AdminMstr.Where(p => p.UserName == objAdminData.UserName || p.ContactNumber == objAdminData.ContactNumber).Any())
                {
                    objInput.success = false;
                    objInput.message = "Admin is already register!!";
                    objInput.Data = null;
                }
                else
                {

                    objAdminData.IsActive = true;
                    objAdminData.IsDeleted = false;
                    objAdminData.CreateDt = DateTime.UtcNow;
                    objAdminData.UpdateDt = DateTime.UtcNow;
                    _dbContext.AdminMstr.Add(objAdminData);
                    _dbContext.SaveChanges();

                    DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objAdminData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Adimn/Remove/{adminId}")]
        public dynamic Adimn_Remove(int adminId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.AdminMstr.Where(p => p.Id == adminId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Admin is not found!!";
                    objInput.Data = null;
                }
                else
                {

                    AdminMstr dbObjAdmin = _dbContext.AdminMstr.Where(p => p.Id == adminId).FirstOrDefault();
                    _dbContext.AdminMstr.Remove(dbObjAdmin);
                    _dbContext.SaveChanges();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbObjAdmin;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Adimn/Edit/{adminId}")]
        public dynamic Adimn_Edit([FromBody]AdminMstr objAdminData, int adminId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.AdminMstr.Where(p => p.Id == adminId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Admin is not found!!";
                    objInput.Data = null;
                }
                else if (_dbContext.AdminMstr.Where(p => p.UserName == objAdminData.UserName || p.ContactNumber == objAdminData.ContactNumber).Any())
                {
                    objInput.success = false;
                    objInput.message = "Admin is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    AdminMstr dbObjAdmin = _dbContext.AdminMstr.Where(p => p.Id == adminId).FirstOrDefault();
                    dbObjAdmin.UserName = objAdminData.UserName == null ? dbObjAdmin.UserName : objAdminData.UserName;
                    dbObjAdmin.Password = objAdminData.Password == null ? dbObjAdmin.Password : objAdminData.Password;
                    dbObjAdmin.FirstName = objAdminData.FirstName == null ? dbObjAdmin.FirstName : objAdminData.FirstName;
                    dbObjAdmin.LastName = objAdminData.LastName == null ? dbObjAdmin.LastName : objAdminData.LastName;
                    dbObjAdmin.ContactNumber = objAdminData.ContactNumber == null ? dbObjAdmin.ContactNumber : objAdminData.ContactNumber;
                    dbObjAdmin.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbObjAdmin;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Adimn/Get/{adminId}")]
        public dynamic Adimn_Get(int adminId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.AdminMstr.Where(p => p.Id == adminId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Admin is not found!!";
                    objInput.Data = null;
                }
                else
                {
                    AdminMstr dbObjAdmin = _dbContext.AdminMstr.Where(p => p.Id == adminId).FirstOrDefault();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbObjAdmin;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Adimn/GetAll")]
        public dynamic Adimn_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.AdminMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Admin is not found!!";
                    objInput.Data = null;
                }
                else
                {
                    var dbObjAdmin = _dbContext.AdminMstr.ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbObjAdmin;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Adimn/Login")]
        public dynamic Adimn_Login([FromBody]AdminMstr objAdminData)
        {
            InputloginData objInput = new InputloginData();
            try
            {
                //AdminMstr UpdateObj = _dbContext.AdminMstr.Single(p => p.ContactNumber == objAdminData.ContactNumber);
                //UpdateObj.Token = objUserData.noti_token;
                //_SellerProductContext.SaveChanges();
                var Data = new List<AdminMstr>();
                if (decimal.TryParse(objAdminData.UserName, out _))
                {
                    Data = _dbContext.AdminMstr.Where(p => (p.ContactNumber == Decimal.Parse(objAdminData.UserName)) && p.Password == objAdminData.Password).ToList();
                }
                else
                {
                    Data = _dbContext.AdminMstr.Where(p => (p.UserName == objAdminData.UserName) && p.Password == objAdminData.Password).ToList();
                }

                if (Data.Count == 0)
                {
                    objInput.status = false;
                    objInput.message = "Username password miss match.";
                    objInput.Data = null;
                }
                else
                {
                    if (Data[0].IsActive)
                    {
                        DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                        string xToken = Data[0].Id.ToString() + "#" + Data[0].UserName + "#" + tomorrow.ToString();

                        objInput.status = true;
                        objInput.message = "User Found Successfully.";
                        objInput.Data = Data[0];
                        objInput.token = Encrypt(xToken);
                    }
                    else
                    {
                        objInput.status = false;
                        objInput.message = "This user is currently deactivate.";
                        objInput.Data = null;
                    }
                }

            }
            catch (Exception ex)
            {
                objInput.status = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            //objHttpCommonResponse.success = objInput.success;
            //objHttpCommonResponse.message = objInput.message;
            //objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objInput);
        }

        [HttpPost]
        [Route("Category/Insert")]
        public dynamic Category_Insert([FromBody]CategoryMstr objCategoryData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (!_dbContext.AdminMstr.Where(p => p.Id == GetAuthId()).Any())
                {
                    objInput.success = false;
                    objInput.message = "you are not able to this!!";
                    objInput.Data = null;
                }
                else if (_dbContext.CategoryMstr.Where(p => p.Name == objCategoryData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "Category code is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    objCategoryData.IsActive = true;
                    objCategoryData.IsDeleted = false;
                    objCategoryData.CreateDt = DateTime.UtcNow;
                    objCategoryData.UpdateDt = DateTime.UtcNow;
                    _dbContext.CategoryMstr.Add(objCategoryData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCategoryData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Category/Edit/{CategoryId}")]
        public dynamic Category_Edit([FromBody]CategoryMstr objCategoryData, int CategoryId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CategoryMstr.Where(p => p.Id == CategoryId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Category is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.CategoryMstr.Where(p => p.Name == objCategoryData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "Category code is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    CategoryMstr dbCategory = _dbContext.CategoryMstr.Find(CategoryId);
                    dbCategory.PCatId = objCategoryData.PCatId == 0 ? dbCategory.PCatId : objCategoryData.PCatId;
                    dbCategory.Name = objCategoryData.Name == null ? dbCategory.Name : objCategoryData.Name;


                    dbCategory.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCategory;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Category/Remove/{CategoryId}")]
        public dynamic Category_Remove(int CategoryId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CategoryMstr.Where(p => p.Id == CategoryId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Category is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CategoryMstr dbCategory = _dbContext.CategoryMstr.Find(CategoryId);
                    _dbContext.CategoryMstr.Remove(dbCategory);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCategory;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Category/Get/{CategoryId}")]
        public dynamic Category_Get(int CategoryId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CategoryMstr.Where(p => p.Id == CategoryId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Category is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CategoryMstr dbCategory = _dbContext.CategoryMstr.Find(CategoryId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCategory;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Category/GetAll")]
        public dynamic Category_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CategoryMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Category is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<CategoryMstr> dbCategory = _dbContext.CategoryMstr.OrderBy(p => p.PCatId).ToList();
                    List<VMCategoryMstr> vMCategories = new List<VMCategoryMstr>();
                    foreach (CategoryMstr cat in dbCategory)
                    {
                        VMCategoryMstr vMCategory = new VMCategoryMstr();
                        vMCategory.Id = cat.Id;
                        vMCategory.PCatId = cat.PCatId;
                        if (cat.PCatId == null)
                        {
                            vMCategory.Name = cat.Name;
                        }
                        else
                        {
                            vMCategory.Name = _dbContext.CategoryMstr.Find(cat.PCatId).Name + " -> " + cat.Name;
                        }
                        vMCategories.Add(vMCategory);
                    }
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = vMCategories;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }





        [HttpPost]
        [Route("Coupon/Insert")]
        public dynamic Coupon_Insert([FromBody]CouponMstr objCouponData)
        {
            InputData objInput = new InputData();
            try
            {

                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                if (_dbContext.CouponMstr.Where(p => p.CoupCode == objCouponData.CoupCode).Any())
                {
                    objInput.success = false;
                    objInput.message = "Coupon code is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    objCouponData.AdminId = GetAuthId();
                    objCouponData.IsActive = true;
                    objCouponData.IsDeleted = false;
                    objCouponData.CreateDt = DateTime.UtcNow;
                    objCouponData.UpdateDt = DateTime.UtcNow;
                    _dbContext.CouponMstr.Add(objCouponData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCouponData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Coupon/Edit/{CouponId}")]
        public dynamic Coupon_Edit([FromBody]CouponMstr objCouponData, int CouponId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CouponMstr.Where(p => p.Id == CouponId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Coupon is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.CouponMstr.Where(p => p.CoupCode == objCouponData.CoupCode && p.Id != CouponId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Coupon code is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    CouponMstr dbCoupon = _dbContext.CouponMstr.Find(CouponId);
                    dbCoupon.AdminId = objCouponData.AdminId == 0 ? dbCoupon.AdminId : objCouponData.AdminId;
                    dbCoupon.CoupCode = objCouponData.CoupCode == null ? dbCoupon.CoupCode : objCouponData.CoupCode;
                    dbCoupon.Name = objCouponData.Name == null ? dbCoupon.Name : objCouponData.Name;
                    dbCoupon.Description = objCouponData.Description == null ? dbCoupon.Description : objCouponData.Description;
                    dbCoupon.DiscountType = objCouponData.DiscountType == 0 ? dbCoupon.DiscountType : objCouponData.DiscountType;
                    dbCoupon.DiscountAmount = objCouponData.DiscountAmount == null ? dbCoupon.DiscountAmount : objCouponData.DiscountAmount;
                    dbCoupon.StartDate = objCouponData.StartDate == null ? dbCoupon.StartDate : objCouponData.StartDate;
                    dbCoupon.EndDate = objCouponData.EndDate == null ? dbCoupon.EndDate : objCouponData.EndDate;



                    dbCoupon.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCoupon;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Coupon/Remove/{CouponId}")]
        public dynamic Coupon_Remove(int CouponId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CouponMstr.Where(p => p.Id == CouponId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Coupon is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CouponMstr dbCoupon = _dbContext.CouponMstr.Find(CouponId);
                    _dbContext.CouponMstr.Remove(dbCoupon);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCoupon;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Coupon/Get/{CouponId}")]
        public dynamic Coupon_Get(int CouponId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CouponMstr.Where(p => p.Id == CouponId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Coupon is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CouponMstr dbCoupon = _dbContext.CouponMstr.Find(CouponId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCoupon;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Coupon/GetAll")]
        public dynamic Coupon_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CouponMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Coupon is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<CouponMstr> dbCoupon = _dbContext.CouponMstr.ToList();
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCoupon;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [Route("Coupon/GetAll/{AdminId}")]
        public dynamic Coupon_GetAll(int AdminId)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CouponMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Coupon is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<CouponMstr> dbCoupon = _dbContext.CouponMstr.Where(p => p.AdminId == AdminId).ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCoupon;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("City/Insert")]
        public dynamic City_Insert([FromBody]CityMstr objCityData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CityMstr.Where(p => p.Name == objCityData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "City is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    objCityData.IsActive = true;
                    objCityData.IsDeleted = false;
                    objCityData.CreateDt = DateTime.UtcNow;
                    objCityData.UpdateDt = DateTime.UtcNow;
                    _dbContext.CityMstr.Add(objCityData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCityData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("City/Edit/{cityId}")]
        public dynamic City_Edit([FromBody]CityMstr objCityData, int cityId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CityMstr.Where(p => p.Id == cityId).Any())
                {
                    objInput.success = false;
                    objInput.message = "City is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.CityMstr.Where(p => p.Name == objCityData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "City is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    CityMstr dbCity = _dbContext.CityMstr.Find(cityId);
                    dbCity.Name = objCityData.Name == null || objCityData.Name == "" ? dbCity.Name : objCityData.Name;
                    dbCity.UpdateDt = DateTime.UtcNow;

                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCity;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("City/Remove/{cityId}")]
        public dynamic City_Remove(int cityId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CityMstr.Where(p => p.Id == cityId).Any())
                {
                    objInput.success = false;
                    objInput.message = "City is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CityMstr dbCity = _dbContext.CityMstr.Find(cityId);
                    _dbContext.CityMstr.Remove(dbCity);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCity;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("City/Get/{cityId}")]
        public dynamic City_Get(int cityId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CityMstr.Where(p => p.Id == cityId).Any())
                {
                    objInput.success = false;
                    objInput.message = "City is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CityMstr dbCity = _dbContext.CityMstr.Find(cityId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCity;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("City/GetAll")]
        public dynamic City_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CityMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "City is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<CityMstr> dbCity = _dbContext.CityMstr.ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCity;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("State/Insert")]
        public dynamic State_Insert([FromBody]StateMstr objStateData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.StateMstr.Where(p => p.Name == objStateData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "State is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    objStateData.IsActive = true;
                    objStateData.IsDeleted = false;
                    objStateData.CreateDt = DateTime.UtcNow;
                    objStateData.UpdateDt = DateTime.UtcNow;
                    _dbContext.StateMstr.Add(objStateData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objStateData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("State/Edit/{StateId}")]
        public dynamic State_Edit([FromBody]StateMstr objStateData, int StateId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.StateMstr.Where(p => p.Id == StateId).Any())
                {
                    objInput.success = false;
                    objInput.message = "State is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.StateMstr.Where(p => p.Name == objStateData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "State is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    StateMstr dbState = _dbContext.StateMstr.Find(StateId);
                    dbState.Name = objStateData.Name == null ? dbState.Name : objStateData.Name;
                    dbState.UpdateDt = DateTime.UtcNow;

                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbState;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("State/Remove/{StateId}")]
        public dynamic State_Remove(int StateId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.StateMstr.Where(p => p.Id == StateId).Any())
                {
                    objInput.success = false;
                    objInput.message = "State is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    StateMstr dbState = _dbContext.StateMstr.Find(StateId);
                    _dbContext.StateMstr.Remove(dbState);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbState;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("State/Get/{StateId}")]
        public dynamic State_Get(int StateId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.StateMstr.Where(p => p.Id == StateId).Any())
                {
                    objInput.success = false;
                    objInput.message = "State is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    StateMstr dbState = _dbContext.StateMstr.Find(StateId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbState;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("State/GetAll")]
        public dynamic State_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.StateMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "State is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<StateMstr> dbState = _dbContext.StateMstr.ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbState;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Country/Insert")]
        public dynamic Country_Insert([FromBody]CountryMstr objCountryData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CountryMstr.Where(p => p.Name == objCountryData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "Country is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    objCountryData.IsActive = true;
                    objCountryData.IsDeleted = false;
                    objCountryData.CreateDt = DateTime.UtcNow;
                    objCountryData.UpdateDt = DateTime.UtcNow;
                    _dbContext.CountryMstr.Add(objCountryData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCountryData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Country/Edit/{CountryId}")]
        public dynamic Country_Edit([FromBody]CountryMstr objCountryData, int CountryId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CountryMstr.Where(p => p.Id == CountryId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Country is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.CountryMstr.Where(p => p.Name == objCountryData.Name).Any())
                {
                    objInput.success = false;
                    objInput.message = "Country is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    CountryMstr dbCountry = _dbContext.CountryMstr.Find(CountryId);
                    dbCountry.Name = objCountryData.Name == null ? dbCountry.Name : objCountryData.Name;
                    dbCountry.UpdateDt = DateTime.UtcNow;

                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCountry;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Country/Remove/{CountryId}")]
        public dynamic Country_Remove(int CountryId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CountryMstr.Where(p => p.Id == CountryId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Country is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CountryMstr dbCountry = _dbContext.CountryMstr.Find(CountryId);
                    _dbContext.CountryMstr.Remove(dbCountry);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCountry;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Country/Get/{CountryId}")]
        public dynamic Country_Get(int CountryId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.CountryMstr.Where(p => p.Id == CountryId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Country is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    CountryMstr dbCountry = _dbContext.CountryMstr.Find(CountryId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCountry;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Country/GetAll")]
        public dynamic Country_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CountryMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Country is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<CountryMstr> dbCountry = _dbContext.CountryMstr.ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbCountry;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Order/ByProduct/GetAll")]
        public dynamic Order_ByProduct_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.OrderTbl.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Order is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<orderByP> orderByPs = new List<orderByP>();
                    var Order_ByProduct = _dbContext.OrderDetailsTbl.OrderBy(p => p.SubProducatId).GroupBy(p => p.SubProducatId);


                    foreach (var gp in Order_ByProduct)
                    {
                        orderByP orderByP = new orderByP();
                        foreach (OrderDetailsTbl item in gp)
                        {
                            var orderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.Id == item.Id).Select(ss => new
                            {
                                pic = ss.SubProducat.ProductImg.FirstOrDefault().Path,
                                name = ss.SubProducat.Product.Name,
                                sku = ss.SubProducat.Product.Sku,
                                price = ss.SubProducat.Price,
                                color = ss.SubProducat.Color.Name,
                                size = ss.SubProducat.Size.Name,
                            }).FirstOrDefault();

                            orderByP.pic = orderDetailsTbl.pic;
                            orderByP.name = orderDetailsTbl.name;
                            orderByP.sku = orderDetailsTbl.sku;
                            orderByP.price = orderDetailsTbl.price;
                            orderByP.color = orderDetailsTbl.color;
                            orderByP.size = orderDetailsTbl.size;

                            orderByP.totalCount += 1;
                        }
                        orderByP.totalAmount = (orderByP.totalCount * orderByP.price);
                        orderByPs.Add(orderByP);
                    }

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = orderByPs;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Order/ByProduct/latest")]
        public dynamic Order_ByProduct_latest()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.OrderTbl.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Order is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<orderByP> orderByPs = new List<orderByP>();
                    var Order_ByProduct = _dbContext.OrderDetailsTbl.Where(p => p.CreateDt > DateTime.Now.AddDays(-30)).OrderBy(p => p.SubProducatId).GroupBy(p => p.SubProducatId);


                    foreach (var gp in Order_ByProduct)
                    {
                        orderByP orderByP = new orderByP();
                        foreach (OrderDetailsTbl item in gp)
                        {
                            var orderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.Id == item.Id).Select(ss => new
                            {
                                pic = ss.SubProducat.ProductImg.FirstOrDefault().Path,
                                name = ss.SubProducat.Product.Name,
                                sku = ss.SubProducat.Product.Sku,
                                price = ss.SubProducat.Price,
                                color = ss.SubProducat.Color.Name,
                                size = ss.SubProducat.Size.Name,
                            }).FirstOrDefault();

                            orderByP.pic = orderDetailsTbl.pic;
                            orderByP.name = orderDetailsTbl.name;
                            orderByP.sku = orderDetailsTbl.sku;
                            orderByP.price = orderDetailsTbl.price;
                            orderByP.color = orderDetailsTbl.color;
                            orderByP.size = orderDetailsTbl.size;

                            orderByP.totalCount += 1;
                        }
                        orderByP.totalAmount = (orderByP.totalCount * orderByP.price);
                        orderByPs.Add(orderByP);
                    }

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = orderByPs;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Order/ByVender/GetAll")]
        public dynamic Order_ByVender_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.OrderTbl.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Order is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<orderByV> orderByvs = new List<orderByV>();
                    List<VenderMstr> Order_Byvender = _dbContext.VenderMstr.OrderBy(p => p.Id).ToList();


                    //var Order_Byvend = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == 1);

                    foreach (VenderMstr v in Order_Byvender)
                    {
                        orderByV orderByV = new orderByV();
                        List<OrderDetailsTbl> Order_Byvend = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == v.Id).ToList();
                        foreach (OrderDetailsTbl o in Order_Byvend)
                        {
                            var orderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.Id == o.Id).Select(ss => new
                            {
                                qty = ss.Qty,
                                price = ss.SubProducat.Price,
                            }).FirstOrDefault();

                            orderByV.totalCount += (int)orderDetailsTbl.qty;
                            orderByV.totalAmount += ((int)orderDetailsTbl.qty * orderDetailsTbl.price);

                            //var orderDetailsTbl = _dbContext.OrderDetailsTbl.Where(p => p.Id == o.Id).Select(ss => new
                            //{
                            //    qty = ss.Qty,
                            //    price = ss.SubProducat.Price,
                            //}).FirstOrDefault();

                            //orderByV.totalCount += (int)o.Qty;
                            //orderByV.totalAmount += ((int)o.Qty * o.SubProducat.Price);
                        }
                        orderByV.displayBusinessName = v.DisplayBusinessName;
                        orderByV.venderFullName = v.VenderFullName;
                        if (orderByV.totalCount != 0)
                        {
                            orderByvs.Add(orderByV);
                        }
                    }



                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = orderByvs;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        #endregion


        #region vender Side Area

        #region vender CRUD 

        [HttpPost]
        [Route("Vender/Insert")]
        public dynamic Vender_Insert([FromBody]VenderMstr objVenderData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.VenderMstr.Where(p => p.MobileNumber == objVenderData.MobileNumber || p.Email == objVenderData.Email).Any())
                {
                    objInput.success = false;
                    objInput.message = "Vender MobileNumber is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    //objVenderData.CityId = 1;
                    //objVenderData.StateId= 1;
                    //objVenderData.CountryId = 1;

                    objVenderData.IsActive = true;
                    objVenderData.IsDeleted = false;
                    objVenderData.CreateDt = DateTime.UtcNow;
                    objVenderData.UpdateDt = DateTime.UtcNow;
                    _dbContext.VenderMstr.Add(objVenderData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objVenderData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Vender/Login")]
        public dynamic Vender_Login([FromBody]VenderMstr objVenderData)
        {
            InputloginData objInput = new InputloginData();

            try
            {
                //VenderMstr UpdateObj = _dbContext.VenderMstr.Single(p => p.ContactNumber == objVenderData.ContactNumber);
                //UpdateObj.Token = objUserData.noti_token;
                //_SellerProductContext.SaveChanges();
                var Data = new List<VenderMstr>();
                if (decimal.TryParse(objVenderData.Email, out _))
                {
                    Data = _dbContext.VenderMstr.Where(p => (p.MobileNumber == Decimal.Parse(objVenderData.Email)) && p.Password == objVenderData.Password).ToList();
                }
                else
                {
                    Data = _dbContext.VenderMstr.Where(p => (p.Email == objVenderData.Email) && p.Password == objVenderData.Password).ToList();
                }

                if (Data.Count == 0)
                {
                    objInput.status = false;
                    objInput.message = "Email or password miss match.";
                    objInput.Data = null;
                }
                else
                {
                    if (Data[0].IsActive)
                    {
                        DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                        string xToken = Data[0].Id.ToString() + "#" + Data[0].Email + "#" + tomorrow.ToString();

                        objInput.status = true;
                        objInput.message = "User Found Successfully.";
                        objInput.Data = Data[0];
                        objInput.token = Encrypt(xToken);
                    }
                    else
                    {
                        objInput.status = false;
                        objInput.message = "This user is currently deactivate.";
                        objInput.Data = null;
                    }
                }

            }
            catch (Exception ex)
            {
                objInput.status = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }



            //objHttpCommonResponse.success = objInput.success;
            //objHttpCommonResponse.message = objInput.message;
            //objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objInput);
        }


        [HttpPost]
        [Route("Vender/Edit/{VenderId}")]
        public dynamic Vender_Edit([FromBody]VenderMstr objVenderData, int VenderId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.VenderMstr.Where(p => p.Id == VenderId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Vender is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.VenderMstr.Where(p => p.MobileNumber == objVenderData.MobileNumber).Any())
                {
                    objInput.success = false;
                    objInput.message = "Vender MobileNumber is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    VenderMstr dbVender = _dbContext.VenderMstr.Find(VenderId);
                    dbVender.MobileNumber = objVenderData.MobileNumber == 0 ? dbVender.MobileNumber : objVenderData.MobileNumber;
                    dbVender.Email = objVenderData.Email == null ? dbVender.Email : objVenderData.Email;
                    dbVender.Password = objVenderData.Password == null ? dbVender.Password : objVenderData.Password;
                    dbVender.DisplayBusinessName = objVenderData.DisplayBusinessName == null ? dbVender.DisplayBusinessName : objVenderData.DisplayBusinessName;
                    dbVender.DisplayBusinessDescription = objVenderData.DisplayBusinessDescription == null ? dbVender.DisplayBusinessDescription : objVenderData.DisplayBusinessDescription;
                    dbVender.VenderFullName = objVenderData.VenderFullName == null ? dbVender.VenderFullName : objVenderData.VenderFullName;
                    dbVender.PreferredTimeSlot = objVenderData.PreferredTimeSlot == null ? dbVender.PreferredTimeSlot : objVenderData.PreferredTimeSlot;
                    dbVender.PreferredLanguage = objVenderData.PreferredLanguage == null ? dbVender.PreferredLanguage : objVenderData.PreferredLanguage;
                    dbVender.AddLine1 = objVenderData.AddLine1 == null ? dbVender.AddLine1 : objVenderData.AddLine1;
                    dbVender.AddLine2 = objVenderData.AddLine2 == null ? dbVender.AddLine2 : objVenderData.AddLine2;
                    dbVender.CityId = objVenderData.CityId == 0 ? dbVender.CityId : objVenderData.CityId;
                    dbVender.StateId = objVenderData.StateId == 0 ? dbVender.StateId : objVenderData.StateId;
                    dbVender.CountryId = objVenderData.CountryId == 0 ? dbVender.CountryId : objVenderData.CountryId;
                    dbVender.Landmark = objVenderData.Landmark == null ? dbVender.Landmark : objVenderData.Landmark;
                    dbVender.PinCode = objVenderData.PinCode == 0 ? dbVender.PinCode : objVenderData.PinCode;

                    dbVender.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbVender;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Vender/Remove/{VenderId}")]
        public dynamic Vender_Remove(int VenderId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.VenderMstr.Where(p => p.Id == VenderId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Vender is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    VenderMstr dbVender = _dbContext.VenderMstr.Find(VenderId);
                    _dbContext.VenderMstr.Remove(dbVender);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbVender;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Vender/Get/{VenderId}")]
        public dynamic Vender_Get(int VenderId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.VenderMstr.Where(p => p.Id == VenderId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Vender is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    VenderMstr dbVender = _dbContext.VenderMstr.Find(VenderId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbVender;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Vender/GetbyAuth")]
        public dynamic Vender_GetbyAuth()
        {
            InputData objInput = new InputData();
            try
            {

                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int VenderId = GetAuthId();
                if (!_dbContext.VenderMstr.Where(p => p.Id == VenderId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Vender is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    VenderMstr dbVender = _dbContext.VenderMstr.Find(VenderId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbVender;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Vender/GetAll")]
        public dynamic Vender_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.VenderMstr.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Vender is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<VenderMstr> dbVender = _dbContext.VenderMstr.ToList();
                    List<venderFadminList> Venderfadmin = new List<venderFadminList>();
                    foreach (VenderMstr dbVenderone in dbVender)
                    {
                        venderFadminList Venderfadminone = new venderFadminList();
                        Venderfadminone.Id = dbVenderone.Id;
                        Venderfadminone.DisplayBusinessName = dbVenderone.DisplayBusinessName;
                        Venderfadminone.VenderFullName = dbVenderone.VenderFullName;
                        Venderfadminone.MobileNumber = dbVenderone.MobileNumber;
                        Venderfadminone.Email = dbVenderone.Email;

                        VenderPaymants ven_paymant = _dbContext.VenderPaymants.Where(p => p.VenderId == dbVenderone.Id).OrderByDescending(p => p.PaymantDate).FirstOrDefault();

                        DateTime lastPdate = default(DateTime);
                        if (ven_paymant != null)
                        {
                            lastPdate = ven_paymant.PaymantDate;
                        }

                        if (lastPdate != default(DateTime))
                        {
                            Venderfadminone.lastPayDate = lastPdate;
                            var od = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == dbVenderone.Id && p.Order.CreateDt > lastPdate && p.VenderPaymantStatus == 1).Select(ss => new
                            {
                                qty = ss.Qty,
                                price = ss.SubProducat.Price,
                            });
                            foreach (var item in od)
                            {
                                Venderfadminone.PaymantAmount += (decimal)(item.qty * item.price);
                            }
                            DateTime curDate = DateTime.Now.Date.AddDays(-15);
                            if (Venderfadminone.PaymantAmount > 0)
                            {
                                if (lastPdate > curDate)
                                {
                                    Venderfadminone.Paymant = 3;
                                }
                                else if (lastPdate < curDate)
                                {

                                    Venderfadminone.Paymant = 1;
                                }
                                else
                                {
                                    Venderfadminone.Paymant = 2;
                                }
                            }
                            else
                            {
                                Venderfadminone.Paymant = 4;
                            }


                        }
                        else
                        {
                            var od = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == dbVenderone.Id && p.VenderPaymantStatus == 1).Select(ss => new
                            {
                                qty = ss.Qty,
                                price = ss.SubProducat.Price,
                            });
                            foreach (var item in od)
                            {
                                Venderfadminone.PaymantAmount += (decimal)(item.qty * item.price);
                            }
                            var jDate = dbVenderone.CreateDt;


                            DateTime curDate = DateTime.Now.Date.AddDays(-15);
                            if (Venderfadminone.PaymantAmount > 0)
                            {
                                if (jDate > curDate)
                                {
                                    Venderfadminone.Paymant = 3;
                                }
                                else if (jDate < curDate)
                                {

                                    Venderfadminone.Paymant = 1;
                                }
                                else
                                {
                                    Venderfadminone.Paymant = 2;
                                }
                            }
                            else
                            {
                                Venderfadminone.Paymant = 4;
                            }

                        }

                        VanderBankDetailsTbl bankd = _dbContext.VanderBankDetailsTbl.Where(p => p.VenderId == dbVenderone.Id).FirstOrDefault();
                        if (bankd != null)
                        {
                            Venderfadminone.AccountHolderName = bankd.AccountHolderName;
                            Venderfadminone.AccountNumber = bankd.AccountNumber;
                            Venderfadminone.IfscCode = bankd.IfscCode;
                            Venderfadminone.BankName = bankd.BankName;
                            Venderfadminone.Branch = bankd.Branch;
                        }
                        Venderfadmin.Add(Venderfadminone);
                    }

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = Venderfadmin;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Vender/PayInsert")]
        public dynamic Vender_Pay_Insert([FromBody]VenderPaymants objVenderPayData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                List<OrderDetailsTbl> OrderD_List = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == objVenderPayData.VenderId).ToList();
                foreach (OrderDetailsTbl item in OrderD_List)
                {
                    item.VenderPaymantStatus = 2;
                }

                objVenderPayData.PaymantDate = DateTime.Now;
                _dbContext.VenderPaymants.Add(objVenderPayData);
                _dbContext.SaveChanges();


                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = objVenderPayData;


            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Vender/PayGetAll")]
        public dynamic Vender_Pay_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                if (_dbContext.VenderPaymants.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "No one hase is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    var dbVenderP = _dbContext.VenderPaymants.OrderByDescending(p => p.PaymantDate).Select(ss => new
                    {
                        id = ss.Id,
                        displayBusinessName = ss.Vender.DisplayBusinessName,
                        venderFullName = ss.Vender.VenderFullName,
                        bankAccount = ss.BankAccount,
                        transid = ss.TransactionId,
                        paymantAmount = ss.Amount,
                        paymantDate = ss.PaymantDate
                    });


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbVenderP;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Vender/GetGrowth/{day}")]
        public dynamic Vender_GetGrowth(int day)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();
                VGrowth vg = new VGrowth();

                DateTime lastdate = DateTime.Now.AddDays(-day);
                List<OrderDetailsTbl> orderDetailsTbls = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == venderid && p.OrderSubStatus == 5 && p.CreateDt > lastdate).ToList();

                foreach (OrderDetailsTbl od in orderDetailsTbls)
                {
                    vg.seleInRupee += (_dbContext.SubProductTbl.Where(p => p.Id == od.SubProducatId).FirstOrDefault().Price * od.Qty);
                    vg.seleInUnint += od.Qty;
                }

                List<OrderDetailsTbl> orederDt = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == venderid && p.OrderSubStatus == 5).ToList();
                List<ReturnTbl> returns = new List<ReturnTbl>();

                foreach (OrderDetailsTbl od in orederDt)
                {
                    vg.returnUint = _dbContext.ReturnTbl.Where(p => p.Order.OrderDetailsTbl.Contains(od) && p.CreateDt > lastdate).Count();
                }

                List<ProductMstr> productMstrs = _dbContext.ProductMstr.Where(p => p.VenderId == venderid).ToList();
                decimal totalrating = 0;

                foreach (ProductMstr product in productMstrs)
                {
                    totalrating = Decimal.Add(totalrating, product.CurrentRating);
                }
                vg.avgrating = Decimal.Divide(totalrating, productMstrs.Count());

                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = vg;

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Vender/PayGetAll/ForMe")]
        public dynamic Vender_Pay_GetAllForMe()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();
                List<VenderPaymants> venderPaymants = _dbContext.VenderPaymants.Where(p => p.VenderId == venderid).ToList();
                if (venderPaymants.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "No one hase is not avalable!!";
                    objInput.Data = null;
                }
                else
                {

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = venderPaymants;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Vender/PayOverView")]
        public dynamic Vender_PayOverView()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                PaymentOverView paymentOverView = new PaymentOverView();

                int venderid = GetAuthId();
                VenderPaymants ven_paymant = _dbContext.VenderPaymants.Where(p => p.VenderId == venderid).OrderByDescending(p => p.PaymantDate).FirstOrDefault();


                DateTime lastPdate = default(DateTime);
                if (ven_paymant != null)
                {
                    lastPdate = ven_paymant.PaymantDate;
                }

                if (lastPdate != default(DateTime))
                {
                    paymentOverView.lastPay_date = lastPdate;
                    paymentOverView.ltotal = ven_paymant.Amount;


                    var od = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == venderid && p.Order.CreateDt > lastPdate && p.VenderPaymantStatus == 1).Select(ss => new
                    {
                        qty = ss.Qty,
                        price = ss.SubProducat.Price,
                    });
                    foreach (var item in od)
                    {
                        paymentOverView.ntotal += (decimal)(item.qty * item.price);
                    }
                    if (paymentOverView.ntotal > 0)
                    {
                        if (lastPdate.AddDays(15) < DateTime.Now)
                        {
                            paymentOverView.nextPay_date = DateTime.Now;
                        }
                        else
                        {
                            paymentOverView.nextPay_date = lastPdate.AddDays(15);
                        }
                    }
                    else
                    {
                        paymentOverView.nextPay_date = DateTime.Now.AddDays(15);
                    }

                }
                else
                {
                    var od = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == venderid && p.VenderPaymantStatus == 1).Select(ss => new
                    {
                        qty = ss.Qty,
                        price = ss.SubProducat.Price,
                    });
                    foreach (var item in od)
                    {
                        paymentOverView.ntotal += (decimal)(item.qty * item.price);
                    }
                    var jDate = _dbContext.VenderMstr.Where(p => p.Id == venderid).FirstOrDefault().CreateDt;

                    if (paymentOverView.ntotal > 0)
                    {
                        if (jDate.AddDays(15) < DateTime.Now)
                        {
                            paymentOverView.nextPay_date = DateTime.Now;
                        }
                        else
                        {
                            paymentOverView.nextPay_date = jDate.AddDays(15);
                        }
                    }
                    else
                    {
                        paymentOverView.nextPay_date = DateTime.Now.AddDays(15);
                    }
                }
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = paymentOverView;
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Vender/MyReturn")]
        public dynamic Vender_MyReturn()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();
                List<ReturnTbl> rs = new List<ReturnTbl>();
                List<returnorderGet> finalreturns = new List<returnorderGet>();

                List<OrderDetailsTbl> orderDetails = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == venderid).ToList();

                foreach (OrderDetailsTbl od in orderDetails)
                {
                    List<ReturnTbl> returns = _dbContext.ReturnTbl.Where(p => p.Order.OrderDetailsTbl.Contains(od)).ToList();
                    foreach (ReturnTbl item in returns)
                    {
                        returnorderGet returnorder = new returnorderGet();
                        SubProductTbl sp = _dbContext.SubProductTbl.Find(od.SubProducatId);
                        UserMstr user = _dbContext.UserMstr.Find(item.UserId);
                        ProductMstr pro = _dbContext.ProductMstr.Find(sp.ProductId);
                        returnorder.id = item.Id;
                        returnorder.sku = pro.Sku;
                        returnorder.productName = pro.Name;
                        returnorder.qty = od.Qty;
                        returnorder.price = sp.Price;
                        returnorder.color = _dbContext.ColoursTbl.Find(sp.ColorId).Name;
                        returnorder.size = _dbContext.SizesTbl.Find(sp.SizeId).Name;
                        returnorder.username = user.FirstName + " " + user.LastName;
                        finalreturns.Add(returnorder);
                    }
                }

                if (finalreturns.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "No one hase is not avalable!!";
                    objInput.Data = null;
                }
                else
                {

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = finalreturns;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Vender/MyOrder")]
        public dynamic Vender_MyOrder()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();

                //List<OrderDetailsTbl> orders1 = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == venderid).ToList();

                //List<OrderDetailsTbl> orders2 = _dbContext.OrderDetailsTbl.Join(_dbContext.OrderTbl,).Where(p => p.SubProducat.Product.VenderId == venderid).Join().ToList();




                var orders3 = _dbContext.OrderDetailsTbl
                    .Join(_dbContext.OrderTbl, od => od.OrderId, o => o.Id, (od, o) => new { od, o })

                    .Where(p => p.od.SubProducat.Product.VenderId == venderid)
                                    .Select(p => new
                                    {
                                        p.od.Id,
                                        p.o.OrderIdV,
                                        p.od.SubProducat.Product.Sku,
                                        p.od.SubProducat.Product.Name,
                                        p.od.Qty,
                                        p.od.SubProducat.Price,
                                        p.o.User.UserName,
                                        p.od.OrderSubStatus,
                                    }).ToList();
                //foreach (var item in orders3)
                //{
                //    vender_orderget orderget = new vender_orderget();
                //    orderget.id = item.od.Id;
                //    orderget.sku = _dbContext.OrderDetailsTbl.Find(item.od.Id).SubProducat.Product.Sku;
                //    fianlorders.Add(orderget);
                //}
                if (orders3.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "No one hase is not avalable!!";
                    objInput.Data = null;
                }
                else
                {

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = orders3;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Vender/ChangeStatusOrder")]
        public dynamic Vender_ChangeStatusOrder([FromBody]List<vender_orderget> objvender_orderget)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (objvender_orderget != null)
                {
                    foreach (vender_orderget item in objvender_orderget)
                    {
                        OrderDetailsTbl orderDet = _dbContext.OrderDetailsTbl.Find(item.Id);
                        if (item.OrderSubStatus < 5)
                        {
                            orderDet.OrderSubStatus = BitConverter.GetBytes(item.OrderSubStatus + 1).First();
                        }
                        OrderTbl order = _dbContext.OrderTbl.Find(orderDet.OrderId);
                        List<OrderDetailsTbl> orderDetails = _dbContext.OrderDetailsTbl.Where(p => p.OrderId == order.Id).ToList();
                        var flag = true;
                        foreach (OrderDetailsTbl od in orderDetails)
                        {
                            if (od.OrderSubStatus != orderDet.OrderSubStatus)
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            order.Status = orderDet.OrderSubStatus;
                        }
                    }
                    _dbContext.SaveChanges();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objvender_orderget;
                }
                else
                {
                    objInput.success = false;
                    objInput.message = "No data for change.  ";
                    objInput.Data = null;
                }
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Vender/RejectOrder")]
        public dynamic Vender_RejectOrder([FromBody]List<vender_orderget> objvender_orderget)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                if (objvender_orderget != null)
                {
                    foreach (vender_orderget item in objvender_orderget)
                    {
                        OrderDetailsTbl orderDet = _dbContext.OrderDetailsTbl.Find(item.Id);
                        orderDet.OrderSubStatus = BitConverter.GetBytes(6).First();
                        OrderTbl order = _dbContext.OrderTbl.Find(orderDet.OrderId);
                        List<OrderDetailsTbl> orderDetails = _dbContext.OrderDetailsTbl.Where(p => p.OrderId == order.Id).ToList();
                        var flag = true;
                        foreach (OrderDetailsTbl od in orderDetails)
                        {
                            if (od.OrderSubStatus != orderDet.OrderSubStatus)
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            order.Status = orderDet.OrderSubStatus;
                        }
                    }
                    _dbContext.SaveChanges();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objvender_orderget;
                }
                else
                {
                    objInput.success = false;
                    objInput.message = "No data for change.  ";
                    objInput.Data = null;
                }
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Vender/Inventory")]
        public dynamic Vender_Inventory()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();

                List<OrderDetailsTbl> orders1 = _dbContext.OrderDetailsTbl.Where(p => p.SubProducat.Product.VenderId == venderid).ToList();

                //List<OrderDetailsTbl> orders2 = _dbContext.OrderDetailsTbl.Join(_dbContext.OrderTbl,).Where(p => p.SubProducat.Product.VenderId == venderid).Join().ToList();




                var stocks = _dbContext.SubProductTbl
                    .Join(_dbContext.ProductMstr, sp => sp.ProductId, p => p.Id, (sp, p) => new { sp, p })
                    .Where(s => s.sp.Product.VenderId == venderid)
                                    .Select(s => new
                                    {
                                        s.sp.Id,
                                        s.p.Sku,
                                        pname = s.p.Name,
                                        s.sp.Qty,
                                        cname = s.sp.Color.Name,
                                        sname = s.sp.Size.Name,
                                    }).ToList();
                //foreach (var item in orders3)
                //{
                //    vender_orderget orderget = new vender_orderget();
                //    orderget.id = item.od.Id;
                //    orderget.sku = _dbContext.OrderDetailsTbl.Find(item.od.Id).SubProducat.Product.Sku;
                //    fianlorders.Add(orderget);
                //}
                if (stocks.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "No one hase is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = stocks;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Vender/AddStock/{SubProductId}")]
        public dynamic Vender_AddStock([FromBody]int addStock, int SubProductId)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }


                SubProductTbl sp = _dbContext.SubProductTbl.Find(SubProductId);
                sp.Qty += addStock;
                _dbContext.SaveChanges();



                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = sp;


            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }



        #endregion

        #region vender color and size 

        [HttpPost]
        [Route("Colours/Insert")]
        public dynamic Colours_Insert([FromBody]ColoursTbl objColoursData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.ColoursTbl.Where(p => (p.VenderId == objColoursData.VenderId && p.Name == objColoursData.Name) && p.VenderId != 0).Any())
                {
                    objInput.success = false;
                    objInput.message = "Colours is already register!!";
                    objInput.Data = null;
                }
                else
                {

                    objColoursData.IsActive = true;
                    objColoursData.IsDeleted = false;
                    objColoursData.CreateDt = DateTime.UtcNow;
                    objColoursData.UpdateDt = DateTime.UtcNow;
                    _dbContext.ColoursTbl.Add(objColoursData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objColoursData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Colours/Edit/{ColoursId}")]
        public dynamic Colours_Edit([FromBody]ColoursTbl objColoursData, int ColoursId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ColoursTbl.Where(p => p.Id == ColoursId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Colours is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.ColoursTbl.Where(p => (p.VenderId == objColoursData.VenderId && p.Name == objColoursData.Name) && p.VenderId != 0).Any())
                {
                    objInput.success = false;
                    objInput.message = "Colours is already register!!";
                    objInput.Data = null;
                }
                else
                {

                    ColoursTbl dbColours = _dbContext.ColoursTbl.Find(ColoursId);
                    dbColours.Name = objColoursData.Name == null ? dbColours.Name : objColoursData.Name;
                    dbColours.VenderId = objColoursData.VenderId == 0 ? dbColours.VenderId : objColoursData.VenderId;

                    dbColours.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbColours;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Colours/Remove/{ColoursId}")]
        public dynamic Colours_Remove(int ColoursId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ColoursTbl.Where(p => p.Id == ColoursId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Colours is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    ColoursTbl dbColours = _dbContext.ColoursTbl.Find(ColoursId);
                    _dbContext.ColoursTbl.Remove(dbColours);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbColours;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Colours/Get/{ColoursId}")]
        public dynamic Colours_Get(int ColoursId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ColoursTbl.Where(p => p.Id == ColoursId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Colours is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    ColoursTbl dbColours = _dbContext.ColoursTbl.Find(ColoursId);

                    dbColours.Vender = _dbContext.VenderMstr.Find(dbColours.VenderId);
                    dbColours.SubProductTbl = _dbContext.SubProductTbl.Where(p => p.SizeId == dbColours.Id).ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbColours;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Colours/GetAll")]
        public dynamic Colours_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.ColoursTbl.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Colours is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<ColoursTbl> dbColours = _dbContext.ColoursTbl.ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbColours;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Sizes/Insert")]
        public dynamic Sizes_Insert([FromBody]SizesTbl objSizesData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.SizesTbl.Where(p => (p.VenderId == objSizesData.VenderId && p.Name == objSizesData.Name) && p.VenderId != 0).Any())
                {
                    objInput.success = false;
                    objInput.message = "Sizes is already register!!";
                    objInput.Data = null;
                }
                else
                {

                    objSizesData.IsActive = true;
                    objSizesData.IsDeleted = false;
                    objSizesData.CreateDt = DateTime.UtcNow;
                    objSizesData.UpdateDt = DateTime.UtcNow;
                    _dbContext.SizesTbl.Add(objSizesData);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objSizesData;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Sizes/Edit/{SizesId}")]
        public dynamic Sizes_Edit([FromBody]SizesTbl objSizesData, int SizesId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.SizesTbl.Where(p => p.Id == SizesId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Sizes is not avalable!!";
                    objInput.Data = null;
                }
                else if (_dbContext.SizesTbl.Where(p => (p.VenderId == objSizesData.VenderId && p.Name == objSizesData.Name) && p.VenderId != 0).Any())
                {
                    objInput.success = false;
                    objInput.message = "Sizes is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    SizesTbl dbSizes = _dbContext.SizesTbl.Find(SizesId);
                    dbSizes.Name = objSizesData.Name == null ? dbSizes.Name : objSizesData.Name;
                    dbSizes.VenderId = objSizesData.VenderId == 0 ? dbSizes.VenderId : objSizesData.VenderId;

                    dbSizes.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbSizes;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Sizes/Remove/{SizesId}")]
        public dynamic Sizes_Remove(int SizesId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.SizesTbl.Where(p => p.Id == SizesId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Sizes is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    SizesTbl dbSizes = _dbContext.SizesTbl.Find(SizesId);
                    _dbContext.SizesTbl.Remove(dbSizes);
                    _dbContext.SaveChanges();


                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbSizes;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Sizes/Get/{SizesId}")]
        public dynamic Sizes_Get(int SizesId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.SizesTbl.Where(p => p.Id == SizesId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Sizes is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    SizesTbl dbSizes = _dbContext.SizesTbl.Find(SizesId);

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbSizes;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpGet]
        [Route("Sizes/GetAll")]
        public dynamic Sizes_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.SizesTbl.Count() <= 0)
                {
                    objInput.success = false;
                    objInput.message = "Sizes is not avalable!!";
                    objInput.Data = null;
                }
                else
                {
                    List<SizesTbl> dbSizes = _dbContext.SizesTbl.ToList();

                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbSizes;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        #endregion

        #region product


        [HttpPost]
        [Route("Product/Insert")]
        public dynamic Product_Insert([FromBody]CustProduct CustProductData)
        {
            InputData objInput = new InputData();
            try
            {

                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();
                if (_dbContext.ProductMstr.Where(p => p.Sku == CustProductData.Sku).Any())
                {
                    objInput.success = false;
                    objInput.message = "Product Sku Already Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change

                    ProductMstr newproduct = new ProductMstr();
                    newproduct.CatId = CustProductData.CatId;
                    newproduct.VenderId = venderid;
                    newproduct.Sku = CustProductData.Sku;
                    newproduct.Name = CustProductData.Name;
                    newproduct.Description = CustProductData.Description;
                    newproduct.Tags = CustProductData.Tags;
                    newproduct.IsReturnable = CustProductData.IsReturnable;
                    newproduct.ReturnDays = CustProductData.ReturnDays;
                    newproduct.Policy = CustProductData.Policy;
                    newproduct.CurrentRating = 0;
                    newproduct.RatingCount = 0;
                    newproduct.ReviewCount = 0;
                    newproduct.UserListing = CustProductData.UserListing;
                    newproduct.PackWeight = CustProductData.PackWeight;
                    newproduct.PackLenght = CustProductData.PackLenght;
                    newproduct.PackBreadth = CustProductData.PackBreadth;
                    newproduct.PackHeight = CustProductData.PackHeight;



                    newproduct.IsActive = true;
                    newproduct.IsDeleted = false;
                    newproduct.CreateDt = DateTime.UtcNow;
                    newproduct.UpdateDt = DateTime.UtcNow;
                    _dbContext.ProductMstr.Add(newproduct);
                    _dbContext.SaveChanges();




                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = newproduct;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }



        [HttpPost]
        [Route("Product/Edit/{ProductId}")]
        public dynamic Product_Edit([FromBody]CustProduct CustProductData, int ProductId)
        {
            InputData objInput = new InputData();
            try
            {
                //if (_dbContext.ProductMstr.Where(p => p.Sku == CustProductData.Sku).Any())
                //{
                //    objInput.success = false;
                //    objInput.message = "Product Sku Already Exist!!";
                //    objInput.Data = null;
                //}
                //else
                //{

                //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change

                ProductMstr newproduct = _dbContext.ProductMstr.Find(ProductId);
                newproduct.CatId = CustProductData.CatId;
                newproduct.Name = CustProductData.Name;
                newproduct.Description = CustProductData.Description;
                newproduct.Tags = CustProductData.Tags;
                newproduct.IsReturnable = CustProductData.IsReturnable;
                newproduct.ReturnDays = CustProductData.ReturnDays;
                newproduct.Policy = CustProductData.Policy;
                newproduct.CurrentRating = CustProductData.CurrentRating;
                newproduct.RatingCount = CustProductData.RatingCount;
                newproduct.ReviewCount = CustProductData.ReviewCount;
                newproduct.UserListing = CustProductData.UserListing;
                newproduct.PackWeight = CustProductData.PackWeight;
                newproduct.PackLenght = CustProductData.PackLenght;
                newproduct.PackBreadth = CustProductData.PackBreadth;
                newproduct.PackHeight = CustProductData.PackHeight;



                newproduct.UpdateDt = DateTime.UtcNow;
                _dbContext.SaveChanges();


                //foreach (CostSubProduct subProduct in CustProductData.SubProductTbl.ToList())
                //{
                //    SubProductTbl newsubProduct = _dbContext.SubProductTbl.Find(subProduct.Id);
                //    newsubProduct.ProductId = newproduct.Id;
                //    newsubProduct.SizeId = subProduct.SizeId;
                //    newsubProduct.ColorId = subProduct.ColorId;
                //    newsubProduct.Price = subProduct.Price;
                //    newsubProduct.OfferPrice = subProduct.OfferPrice;
                //    newsubProduct.Qty = subProduct.Qty;

                //    newsubProduct.UpdateDt = DateTime.UtcNow;
                //    _dbContext.SaveChanges();

                //    foreach (CustProductImg pimg in subProduct.ProductImg.ToList())
                //    {
                //        ProductImg newProImg = _dbContext.ProductImg.Find(pimg.Id);
                //        newProImg.SubProducatId = newsubProduct.Id;
                //        newProImg.Path = pimg.Path;
                //        newProImg.UpdateDt = DateTime.UtcNow;
                //        _dbContext.SaveChanges();
                //    }

                //}


                //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                //objInput.AuthToken = Encrypt(xToken);
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = newproduct;
                //}

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Product/Remove/{ProductId}")]
        public dynamic Product_Remove(int ProductId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ProductMstr.Where(p => p.Id == ProductId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Product is not Exist!!";
                    objInput.Data = null;
                }
                else if (_dbContext.ProductMstr.Find(ProductId).IsDeleted == true)
                {
                    objInput.success = false;
                    objInput.message = "Product is alrady deleted!!";
                    objInput.Data = null;
                }
                else
                {

                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change

                    ProductMstr newproduct = _dbContext.ProductMstr.Find(ProductId);
                    _dbContext.ProductMstr.Remove(newproduct);
                    _dbContext.SaveChanges();


                    foreach (SubProductTbl subProduct in _dbContext.SubProductTbl.Where(p => p.ProductId == newproduct.Id).ToList())
                    {

                        _dbContext.SubProductTbl.Remove(subProduct);
                        _dbContext.SaveChanges();

                        foreach (ProductImg pimg in _dbContext.ProductImg.Where(p => p.SubProducatId == subProduct.Id).ToList())
                        {
                            _dbContext.ProductImg.Remove(pimg);
                            _dbContext.SaveChanges();
                        }

                    }


                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = newproduct;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Product/Get/{ProductId}")]
        public dynamic Product_Get(int ProductId)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ProductMstr.Where(p => p.Id == ProductId && p.IsDeleted == false).Any())
                {
                    objInput.success = false;
                    objInput.message = "Product is not Exist!!";
                    objInput.Data = null;
                }


                else
                {

                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change

                    ProductMstr newproduct = _dbContext.ProductMstr.Find(ProductId);


                    foreach (SubProductTbl subProduct in _dbContext.SubProductTbl.Where(p => p.ProductId == newproduct.Id).ToList())
                    {
                        foreach (ProductImg pimg in _dbContext.ProductImg.Where(p => p.SubProducatId == subProduct.Id).ToList())
                        {
                            subProduct.ProductImg.Add(pimg);
                        }
                        subProduct.Color = _dbContext.ColoursTbl.Find(subProduct.ColorId);
                        subProduct.Size = _dbContext.SizesTbl.Find(subProduct.SizeId);
                        newproduct.SubProductTbl.Add(subProduct);
                    }


                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = newproduct;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Product/GetAll")]
        public dynamic Product_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ProductMstr.ToList().Any())
                {
                    objInput.success = false;
                    objInput.message = "Product is not Exist!!";
                    objInput.Data = null;
                }

                else
                {

                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change

                    List<ProductMstr> productList = _dbContext.ProductMstr.Where(p => p.IsDeleted == false).ToList();
                    List<CustProduct> sendProduct = new List<CustProduct>();
                    foreach (ProductMstr newProduct in productList)
                    {
                        CustProduct cp = new CustProduct();
                        cp.CatId = newProduct.CatId;
                        cp.VenderId = newProduct.VenderId;
                        cp.Sku = newProduct.Sku;
                        cp.Name = newProduct.Name;
                        cp.Description = newProduct.Description;
                        cp.Tags = newProduct.Tags;
                        cp.IsReturnable = newProduct.IsReturnable;
                        cp.ReturnDays = newProduct.ReturnDays;
                        cp.Policy = newProduct.Policy;
                        cp.CurrentRating = 0;
                        cp.RatingCount = 0;
                        cp.ReviewCount = 0;
                        cp.UserListing = newProduct.UserListing;
                        cp.PackWeight = newProduct.PackWeight;
                        cp.PackLenght = newProduct.PackLenght;
                        cp.PackBreadth = newProduct.PackBreadth;
                        cp.PackHeight = newProduct.PackHeight;

                        foreach (SubProductTbl newsubProduct in _dbContext.SubProductTbl.Where(p => p.ProductId == cp.Id).ToList())
                        {
                            CostSubProduct csp = new CostSubProduct();
                            csp.ProductId = newsubProduct.ProductId;
                            csp.SizeId = newsubProduct.SizeId;
                            csp.ColorId = newsubProduct.ColorId;
                            csp.Price = newsubProduct.Price;
                            csp.OfferPrice = newsubProduct.OfferPrice;
                            csp.Qty = newsubProduct.Qty;
                            foreach (ProductImg pimg in _dbContext.ProductImg.Where(p => p.SubProducatId == csp.Id).ToList())
                            {
                                CustProductImg cProImg = new CustProductImg();
                                cProImg.SubProducatId = newsubProduct.Id;
                                cProImg.Path = pimg.Path;
                                csp.ProductImg.Add(cProImg);
                            }
                            cp.SubProductTbl.Add(csp);
                        }

                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = productList;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpGet]
        [Route("Product/Search/{searchText}")]
        public dynamic Product_Search(string searchText)
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ProductMstr.Where(p => (p.Name.Contains(searchText) || p.Sku.Contains(searchText) || p.Description.Contains(searchText) || p.Tags.Contains(searchText)) && p.IsDeleted == false && p.SubProductTbl.Count > 0 && p.UserListing).ToList().Any())
                {
                    objInput.success = false;
                    objInput.message = "Product is not Found for \" "+ searchText + " \"!!";
                    objInput.Data = null;
                }

                else
                {

                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change

                    List<ProductMstr> productList = _dbContext.ProductMstr.Where(p => (p.Name.Contains(searchText) || p.Sku.Contains(searchText) || p.Description.Contains(searchText) || p.Tags.Contains(searchText)) && p.IsDeleted == false && p.SubProductTbl.Count > 0 && p.UserListing).ToList();
                    foreach (ProductMstr pro in productList)
                    {
                        pro.SubProductTbl = _dbContext.SubProductTbl.Where(p => p.ProductId == pro.Id).ToList();
                        foreach (SubProductTbl subPro in pro.SubProductTbl)
                        {
                            subPro.Color = _dbContext.ColoursTbl.Find(subPro.ColorId);
                            subPro.ProductImg = _dbContext.ProductImg.Where(p => p.SubProducatId == subPro.Id).ToList();
                        }
                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = productList;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }



        [HttpGet]
        [Route("subProduct/Getall/{productid}")]
        public dynamic subProduct_Getall(int productid)
        {
            InputData objInput = new InputData();
            try
            {


                int venderid = GetAuthId();
                if (!_dbContext.SubProductTbl.Where(p => p.ProductId == productid).Any())
                {
                    objInput.success = false;
                    objInput.message = "sub Product not Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change


                    List<SubProductTbl> subProducts = _dbContext.SubProductTbl.Where(p => p.ProductId == productid).ToList();
                    foreach (SubProductTbl subProduct in subProducts)
                    {
                        subProduct.Color = _dbContext.ColoursTbl.Find(subProduct.ColorId);
                        subProduct.Size = _dbContext.SizesTbl.Find(subProduct.SizeId);
                        foreach (ProductImg pimg in _dbContext.ProductImg.Where(p => p.SubProducatId == subProduct.Id).ToList())
                        {
                            subProduct.ProductImg.Add(pimg);
                        }
                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = subProducts;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("subProduct/Insert/{productid}")]
        public dynamic subProduct_Insert([FromBody]CostSubProduct CustsubProductData, int productid)
        {
            InputData objInput = new InputData();
            try
            {

                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();
                if (_dbContext.SubProductTbl.Where(p => p.ProductId == productid && p.SizeId == CustsubProductData.SizeId && p.ColorId == CustsubProductData.ColorId).Any())
                {
                    objInput.success = false;
                    objInput.message = "sub Product Already Exist!!";
                    objInput.Data = null;
                }
                else
                {

                    SubProductTbl newsubProduct = new SubProductTbl();
                    newsubProduct.ProductId = productid;
                    newsubProduct.SizeId = CustsubProductData.SizeId;
                    newsubProduct.ColorId = CustsubProductData.ColorId;
                    newsubProduct.Price = CustsubProductData.Price;
                    newsubProduct.OfferPrice = CustsubProductData.OfferPrice;
                    newsubProduct.Qty = CustsubProductData.Qty;

                    newsubProduct.IsActive = true;
                    newsubProduct.IsDeleted = false;
                    newsubProduct.CreateDt = DateTime.UtcNow;
                    newsubProduct.UpdateDt = DateTime.UtcNow;
                    _dbContext.SubProductTbl.Add(newsubProduct);

                    if (CustsubProductData.ProductImg.Count > 0)
                    {
                        _dbContext.SaveChanges();

                        foreach (CustProductImg pimg in CustsubProductData.ProductImg.ToList())
                        {

                            ProductImg newProImg = new ProductImg();
                            newProImg.SubProducatId = newsubProduct.Id;
                            newProImg.Path = pimg.Path.Split("/")[1];
                            newProImg.IsActive = true;
                            newProImg.IsDeleted = false;
                            newProImg.CreateDt = DateTime.UtcNow;
                            newProImg.UpdateDt = DateTime.UtcNow;
                            _dbContext.ProductImg.Add(newProImg);

                            if (!System.IO.File.Exists(Path.Combine(ImgPath, newProImg.Path)) && newProImg.Path != "")
                                System.IO.File.Move(Path.Combine(TempPath, newProImg.Path), Path.Combine(ImgPath, newProImg.Path));
                        }

                        _dbContext.SaveChanges();

                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = newsubProduct;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }



        [HttpPost]
        [Route("subProduct/Edit/{Subproductid}")]
        public dynamic subProduct_Edit([FromBody]CostSubProduct CustsubProductData, int Subproductid)
        {
            InputData objInput = new InputData();
            try
            {

                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int venderid = GetAuthId();
                if (_dbContext.SubProductTbl.Where(p => p.ProductId == CustsubProductData.ProductId && p.SizeId == CustsubProductData.SizeId && p.ColorId == CustsubProductData.ColorId && p.Id != Subproductid).Any())
                {
                    objInput.success = false;
                    objInput.message = "sub Product Already Exist!!";
                    objInput.Data = null;
                }
                else
                {

                    SubProductTbl newsubProduct = _dbContext.SubProductTbl.Find(Subproductid);
                    newsubProduct.SizeId = CustsubProductData.SizeId;
                    newsubProduct.ColorId = CustsubProductData.ColorId;
                    newsubProduct.Price = CustsubProductData.Price;
                    newsubProduct.OfferPrice = CustsubProductData.OfferPrice;
                    newsubProduct.Qty = CustsubProductData.Qty;

                    newsubProduct.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    if (CustsubProductData.ProductImg.Count > 0)
                    {

                        foreach (CustProductImg pimg in CustsubProductData.ProductImg.ToList())
                        {
                            var locate = pimg.Path.Split("/")[0];
                            var imgname = pimg.Path.Split("/")[1];
                            if (locate == "temp")
                            {
                                if (!System.IO.File.Exists(Path.Combine(ImgPath, imgname)) && pimg.Path != "")
                                {

                                    ProductImg newProImg = new ProductImg();
                                    newProImg.SubProducatId = newsubProduct.Id;
                                    newProImg.Path = imgname;
                                    newProImg.IsActive = true;
                                    newProImg.IsDeleted = false;
                                    newProImg.CreateDt = DateTime.UtcNow;
                                    newProImg.UpdateDt = DateTime.UtcNow;
                                    System.IO.File.Move(Path.Combine(TempPath, imgname), Path.Combine(ImgPath, imgname));
                                    _dbContext.ProductImg.Add(newProImg);
                                }

                            }
                            else if (locate == "subproduct")
                            {
                                if (!_dbContext.ProductImg.Where(p => p.SubProducatId == Subproductid && p.Path == imgname).Any())
                                {
                                    var abc = "abc";
                                }
                            }
                        }

                        _dbContext.SaveChanges();

                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = newsubProduct;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("subProduct/productImg/Remove/{Subproductid}")]
        public dynamic subProduct_IMG_Remove([FromBody]CustProductImg ImagPath, int Subproductid)
        {
            InputData objInput = new InputData();
            try
            {
                var locate = ImagPath.Path.Split("/")[0];
                var imgname = ImagPath.Path.Split("/")[1];
                if (locate == "temp")
                {
                    if (System.IO.File.Exists(Path.Combine(TempPath, imgname)) && ImagPath.Path != "")
                    {
                        System.IO.File.Delete(Path.Combine(TempPath, imgname));
                    }

                }
                else if (locate == "subproduct")
                {
                    if (System.IO.File.Exists(Path.Combine(ImgPath, imgname)) && ImagPath.Path != "")
                    {
                        ProductImg dimg = _dbContext.ProductImg.Where(p => p.Path == imgname && p.SubProducatId == Subproductid).First();
                        _dbContext.ProductImg.Remove(dimg);
                        _dbContext.SaveChanges();
                        System.IO.File.Delete(Path.Combine(ImgPath, imgname));
                    }
                }



                //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                //objInput.AuthToken = Encrypt(xToken);
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = ImagPath;

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }





        [HttpGet]
        [Route("Product/GetAll/Vender")]
        public dynamic Product_GetAll_forVendor()
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                int VenderId = GetAuthId();
                if (!_dbContext.ProductMstr.Where(p => p.VenderId == VenderId && p.IsDeleted == false).Any())
                {
                    objInput.success = false;
                    objInput.message = "Product is not Exist for you!!";
                    objInput.Data = null;
                }

                else
                {

                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change


                    List<ProductMstr> productList = _dbContext.ProductMstr.Where(p => p.VenderId == VenderId && p.IsDeleted == false).ToList();
                    List<ProductForVender> sendProList = new List<ProductForVender>();
                    foreach (ProductMstr newProduct in productList)
                    {
                        ProductForVender POV = new ProductForVender();
                        POV.Id = newProduct.Id;
                        POV.pic = _dbContext.ProductImg.Where(p => p.SubProducat.Product.Id == newProduct.Id).First().Path;
                        POV.sku = newProduct.Sku;
                        POV.name = newProduct.Name;
                        CategoryMstr cats = _dbContext.CategoryMstr.Find(newProduct.CatId);

                        if (cats.PCatId == null)
                        {
                            POV.catname = cats.Name;
                        }
                        else
                        {
                            POV.catname = _dbContext.CategoryMstr.Find(cats.PCatId).Name + " -> " + cats.Name;
                        }
                        POV.CurrentRating = newProduct.CurrentRating;
                        POV.UserListing = newProduct.UserListing;
                        POV.ColoursList = new List<string>();
                        foreach (SubProductTbl newsubProduct in _dbContext.SubProductTbl.Where(p => p.ProductId == POV.Id).ToList())
                        {
                            POV.ColoursList.Add(_dbContext.ColoursTbl.Find(newsubProduct.ColorId).Name);
                        }
                        sendProList.Add(POV);
                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = sendProList;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Product/GetAll/Fordeal")]
        public dynamic Product_GetAll_Fordeal()
        {
            InputData objInput = new InputData();
            try
            {
                if (!_dbContext.ProductMstr.ToList().Any())
                {
                    objInput.success = false;
                    objInput.message = "Product is not Exist!!";
                    objInput.Data = null;
                }

                else
                {

                    //(5 * 252 + 4 * 124 + 3 * 40 + 2 * 29 + 1 * 33) / (252 + 124 + 40 + 29 + 33) = 4.11 and change

                    List<ProductMstr> productList = _dbContext.ProductMstr.Where(p => p.IsActive == true).ToList();
                    List<product_for_deal> productdeal = new List<product_for_deal>();
                    foreach (ProductMstr newproduct in productList)
                    {
                        newproduct.Cat = _dbContext.CategoryMstr.Find(newproduct.CatId);
                        product_for_deal _pro = new product_for_deal();
                        _pro.id = newproduct.Id;
                        _pro.vid = newproduct.VenderId;
                        _pro.sku = newproduct.Sku;
                        _pro.name = newproduct.Name;
                        _pro.currentRating = newproduct.CurrentRating;
                        _pro.ratingCount = newproduct.RatingCount;
                        _pro.cat = _dbContext.CategoryMstr.Find(newproduct.CatId).Name;
                        productdeal.Add(_pro);
                    }

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objAdminData.Id.ToString() + "#" + objAdminData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = productdeal;
                }

            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            //objHttpCommonResponse.AuthToken = objInput.AuthToken;
            return Ok(objHttpCommonResponse);
        }

        [HttpPost]
        [Route("Deal/Insert")]
        public dynamic Deal_Insert([FromBody]dealTransfer objDealData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                foreach (product_for_deal product_For_Deal in objDealData.ProList)
                {
                    TodayDealsTbl deal = new TodayDealsTbl();
                    deal.ProId = product_For_Deal.id;
                    deal.AdminId = objDealData.AdminId = GetAuthId();
                    deal.DiscountType = objDealData.DiscountType;
                    deal.DiscountAmount = objDealData.DiscountAmount;
                    deal.StartDate = objDealData.StartDate;
                    deal.IsActive = true;
                    deal.IsDeleted = false;
                    deal.CreateDt = DateTime.UtcNow;
                    deal.UpdateDt = DateTime.UtcNow;
                    _dbContext.TodayDealsTbl.Add(deal);
                    _dbContext.SaveChanges();
                }
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = objDealData;
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }

        [HttpGet]
        [Route("Deal/GetAll")]
        public dynamic Deal_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                List<TodayDealsTbl> dealLs = _dbContext.TodayDealsTbl.Where(p => p.StartDate > DateTime.Now.AddDays(-1)).GroupBy(pg => new
                {
                    pg.AdminId,
                    pg.StartDate,
                    pg.DiscountType,
                    pg.DiscountAmount,
                }).Select(
                    gcs => new TodayDealsTbl()
                    {
                        AdminId = gcs.Key.AdminId,
                        StartDate = gcs.Key.StartDate,
                        DiscountAmount = gcs.Key.DiscountAmount,
                        DiscountType = gcs.Key.DiscountType
                    }).ToList();
                List<dealTransfer> dealTransfer = new List<dealTransfer>();
                foreach (TodayDealsTbl deal in dealLs)
                {
                    dealTransfer dealTransfer1 = new dealTransfer();
                    dealTransfer1.AdminId = deal.AdminId;
                    dealTransfer1.DiscountType = deal.DiscountType;
                    dealTransfer1.DiscountAmount = deal.DiscountAmount;
                    dealTransfer1.StartDate = deal.StartDate;
                    dealTransfer1.ProList = new List<product_for_deal>();
                    List<TodayDealsTbl> alldeal = _dbContext.TodayDealsTbl.Where(p => p.StartDate == deal.StartDate && p.DiscountType == deal.DiscountType && p.DiscountAmount == deal.DiscountAmount).ToList();
                    foreach (TodayDealsTbl onedeal in alldeal)
                    {
                        ProductMstr product = _dbContext.ProductMstr.Find(onedeal.ProId);
                        product_for_deal product_For = new product_for_deal();
                        product_For.vid = product.VenderId;
                        product_For.sku = product.Sku;
                        product_For.name = product.Name;
                        product_For.currentRating = product.CurrentRating;
                        product_For.ratingCount = product.RatingCount;
                        product_For.cat = _dbContext.CategoryMstr.Find(product.CatId).Name;

                        dealTransfer1.ProList.Add(product_For);
                    }

                    dealTransfer.Add(dealTransfer1);
                }

                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = dealTransfer;
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        [HttpPost]
        [Route("Deal/Edit")]
        public dynamic Deal_Edit([FromBody]dealTransfer[] objDealData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                List<TodayDealsTbl> dealLs = _dbContext.TodayDealsTbl.Where(p => p.StartDate == objDealData[0].StartDate && p.DiscountType == objDealData[0].DiscountType && p.DiscountAmount == objDealData[0].DiscountAmount).ToList();
                foreach (TodayDealsTbl deals in dealLs)
                {
                    deals.StartDate = objDealData[1].StartDate;
                    deals.DiscountType = objDealData[1].DiscountType;
                    deals.DiscountAmount = objDealData[1].DiscountAmount;
                    deals.UpdateDt = DateTime.Now;
                    _dbContext.SaveChanges();
                }
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = dealLs;
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }
        [HttpPost]
        [Route("Deal/Remove")]
        public dynamic Deal_Remove([FromBody]dealTransfer objDealData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }

                List<TodayDealsTbl> dealLs = _dbContext.TodayDealsTbl.Where(p => p.StartDate == objDealData.StartDate && p.DiscountType == objDealData.DiscountType && p.DiscountAmount == objDealData.DiscountAmount).ToList();

                foreach (TodayDealsTbl deals in dealLs)
                {
                    _dbContext.TodayDealsTbl.Remove(deals);
                    _dbContext.SaveChanges();

                }
                objInput.success = true;
                objInput.message = "Successfully. ";
                objInput.Data = dealLs;
            }
            catch (Exception ex)
            {
                objInput.success = false;
                objInput.message = "Something went wrong, please contact support";
                objInput.Data = null;
            }

            objHttpCommonResponse.success = objInput.success;
            objHttpCommonResponse.message = objInput.message;
            objHttpCommonResponse.data = objInput.Data;
            return Ok(objHttpCommonResponse);
        }


        #endregion

        #endregion


        #region othermethods

        static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        ~ShopAPIController()
        {
            _dbContext.Dispose();
        }

        bool ValidateToken(ref InputData objInput)
        {
            objInput.success = false;
            objInput.message = "Invalid auth token, please try login again.";
            if (GetExpiryDate() >= DateTime.UtcNow)
            {
                objInput.success = true;
                objInput.message = "Valid auth token";
                return true;
            }
            objInput.status = "Unauthorized";
            return false;
        }

        string GetAuthToken()
        {
            try
            {
                string[] strString = GetToken().Split('#');
                return strString[1];
            }
            catch (Exception ex)
            {
                //objLogs.WriteLog("AccountController", "GetToken", ex.ToString());
            }
            return "";
        }

        DateTime GetExpiryDate()
        {
            try
            {
                string[] strString = GetToken().Split('#');
                return DateTime.Parse(strString[2]);
            }
            catch (Exception ex)
            {
                //objLogs.WriteLog("AccountController", "GetToken", ex.ToString());
            }
            return DateTime.UtcNow.AddDays(-5);
        }

        int GetAuthId()
        {
            int userId = 0;
            try
            {
                string[] strString = GetToken().Split('#');
                Int32.TryParse(strString[0], out userId);
            }
            catch (Exception ex)
            {
                //objLogs.WriteLog("AccountController", "GetToken", ex.ToString());
            }
            return userId;
        }

        string GetToken()
        {
            string token = "0:0";
            try
            {
                //HttpRequestHeaders coll = Request.Headers;
                //IEnumerable<string> str = coll.GetValues("X-Auth");
                var xyz = HttpContext.Request?.Headers["Authorization"];
                // Load Header collection into NameValueCollection object.
                //foreach (var item in xyz)
                //{
                //token = item.ToString();
                token = Decrypt(xyz);
                //}
            }
            catch (Exception ex)
            {
                //objLogs.WriteLog("AccountController", "GetToken", ex.ToString());
            }

            return token;
        }

        string Decrypt(string encryptedToken)
        {
            return Decrypt64(encryptedToken);
        }

        string Encrypt(string token)
        {
            return Encrypt64(token);
        }

        string Encrypt64(string stringToEncrypt)
        {
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream;

                des.Key = HashKey(DEFAULT_KEY, des.KeySize / 8);
                des.IV = HashKey(DEFAULT_KEY, des.KeySize / 8);
                byte[] inputBytes = Encoding.UTF32.GetBytes(stringToEncrypt);

                cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                cryptoStream.FlushFinalBlock();
                string tempStr = Convert.ToBase64String(memoryStream.ToArray());
                tempStr = tempStr.Replace('+', '~');
                //tempStr = tempStr.Replace('=', '_');
                return tempStr;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        string Decrypt64(string stringToDecrypt)
        {
            try
            {
                stringToDecrypt = stringToDecrypt.Replace('~', '+');
                //stringToDecrypt = stringToDecrypt.Replace('_', '=');
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream;
                // Check whether the key is valid, otherwise make it valid
                // CheckKey(ref key);
                des.Key = HashKey(DEFAULT_KEY, des.KeySize / 8);
                des.IV = HashKey(DEFAULT_KEY, des.KeySize / 8);
                byte[] inputBytes = Convert.FromBase64String(stringToDecrypt);
                cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                cryptoStream.FlushFinalBlock();
                Encoding encoding = Encoding.UTF32;
                return encoding.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        void CheckKey(ref string keyToCheck)
        {
            keyToCheck = keyToCheck.Length > 8 ? keyToCheck.Substring(0, 8) : keyToCheck;
            if (keyToCheck.Length < 8)
            {
                for (int i = keyToCheck.Length; i < 8; i++)
                {
                    keyToCheck += DEFAULT_KEY[i];
                }
            }
        }

        byte[] HashKey(string key, int length)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            // Hash the key
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] hash = sha1.ComputeHash(keyBytes);
            // Truncate hash
            byte[] truncatedHash = new byte[length];
            Array.Copy(hash, 0, truncatedHash, 0, length);
            return truncatedHash;
        }

        void sendMail(string emailid,string subject,string mailbody)
        {
            MimeMessage message = new MimeMessage();
            MailboxAddress from = new MailboxAddress("Doorja Support Team", "doorjashopcart@gmail.com");
            message.From.Add(from);
            MailboxAddress to = new MailboxAddress(emailid);
            message.To.Add(to);
            message.Subject = subject;
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = mailbody;
            message.Body = bodyBuilder.ToMessageBody();
            SmtpClient client = new SmtpClient();
            client.Connect("smtp.gmail.com", 465, true);
            client.Authenticate("doorjashopcart@gmail.com", "GMnitin1112");
            client.Send(message);
            client.Disconnect(true);
            client.Dispose();
        }

        #endregion





        public class InputData
        {
            public bool isedit { get; set; }
            public string AuthToken { get; set; }
            public string status { get; set; }
            public bool? success { get; set; }
            public string message { get; set; }
            public DateTime timestamp { get; set; }
            public dynamic Data { get; set; }
            public string id { get; set; }
            public string ProductId { get; set; }
        }
        public class InputloginData
        {
            public bool status { get; set; }
            public dynamic Data { get; set; }
            public string token { get; set; }
            public string message { get; set; }
        }
        public class MetaData
        {
            public int? totalCount { get; set; }
            public int? start { get; set; }
            public int? count { get; set; }
        }
        public class HttpCommonResponse
        {
            public string AuthToken { get; set; }
            public bool? success { get; set; }
            public bool isedit { get; set; }
            public string message { get; set; }
            public DateTime timestamp { get; set; }
            public object data { get; set; }
            //TODO: update to Naming to metaData
            public MetaData metadata { get; set; }
        }


    }

}
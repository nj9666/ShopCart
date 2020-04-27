using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopCart.Models;

using System.Net.Http.Handlers;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace ShopCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopAPIController : ControllerBase
    {

        private const string DEFAULT_KEY = "#kl?~@<z";

        private DemoCratContext _dbContext;

        HttpCommonResponse objHttpCommonResponse = new HttpCommonResponse();

        public ShopAPIController(DemoCratContext context)
        {
            _dbContext = context;
        }


        #region User Area


        [HttpPost]
        [Route("User/Registration")]
        public dynamic User_Registration([FromBody]UserMstr objUserData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.UserMstr.Where(p => p.ContactNumber == objUserData.ContactNumber).Any())
                {
                    objInput.success = false;
                    objInput.message = "Phone Number already register!!";
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
                    objInput.message = "Successfully. ";
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
        [Route("User/Edit/{UserId}")]
        public dynamic User_Edit([FromBody]UserMstr objUserData,int UserId)
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
                else if (_dbContext.UserMstr.Where(p => p.ContactNumber == objUserData.ContactNumber).Any())
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
                    dbobjUser.Gender = objUserData.Gender == false ? dbobjUser.Gender : objUserData.Gender;
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








        #region card Area

        [HttpPost]
        [Route("Card/Insert")]
        public dynamic Card_Insert([FromBody]CardsTbl objCardsData)
        {
            InputData objInput = new InputData();
            try
            {
                InputData objInputdd = new InputData();
                if (!ValidateToken(ref objInputdd))
                {
                    return Unauthorized();
                }
                if (_dbContext.CardsTbl.Where(p => p.CardNumber == objCardsData.CardNumber && p.UserId == objCardsData.UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cards is already register!!";
                    objInput.Data = null;
                }
                else
                {

                    objCardsData.IsActive = true;
                    objCardsData.IsDeleted = false;
                    objCardsData.CreateDt = DateTime.UtcNow;
                    objCardsData.UpdateDt = DateTime.UtcNow;
                    _dbContext.CardsTbl.Add(objCardsData);
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCardsData;
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
        [Route("Card/Edit/{CardId}")]
        public dynamic Card_Edit([FromBody]CardsTbl objCardsData,int CardId)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CardsTbl.Where(p => p.Id == CardId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cards is ont Exist!!";
                    objInput.Data = null;
                }
                else if (_dbContext.CardsTbl.Where(p => p.CardNumber == objCardsData.CardNumber && p.UserId == objCardsData.UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cards is already register!!";
                    objInput.Data = null;
                }
                else
                {
                    CardsTbl dbobjCards = _dbContext.CardsTbl.Find(CardId);
                    dbobjCards.CardNumber = objCardsData.CardNumber == 0 ? dbobjCards.CardNumber : objCardsData.CardNumber;
                    dbobjCards.ExMonth = objCardsData.ExMonth == 0 ? dbobjCards.ExMonth : objCardsData.ExMonth;
                    dbobjCards.ExYear = objCardsData.ExYear == 0 ? dbobjCards.ExYear : objCardsData.ExYear;
                    dbobjCards.HolderName = objCardsData.HolderName == null ? dbobjCards.HolderName : objCardsData.HolderName;
                    dbobjCards.CardLabel = objCardsData.CardLabel == null ? dbobjCards.CardLabel : objCardsData.CardLabel;
                    dbobjCards.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = dbobjCards;
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
        [Route("Card/Remove/{CardId}")]
        public dynamic Card_Remove(int CardId)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CardsTbl.Where(p => p.Id == CardId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cards is ont Exist!!";
                    objInput.Data = null;
                }
                
                else
                {
                    CardsTbl objCardsData = _dbContext.CardsTbl.Find(CardId);
                    objCardsData.IsActive = false;
                    objCardsData.IsDeleted = true;
                    objCardsData.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCardsData;
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
        [Route("Card/Get/{CardId}")]
        public dynamic Card_Get(int CardId)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CardsTbl.Where(p => p.Id == CardId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cards is ont Exist!!";
                    objInput.Data = null;
                }

                else
                {
                    CardsTbl objCardsData = _dbContext.CardsTbl.Find(CardId);
                   
                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCardsData;
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
        [Route("Card/GetAll/My")]
        public dynamic Card_GetAll()
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = 2;
                if (!_dbContext.CardsTbl.Where(p => p.UserId == UserId).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cards is ont Exist!!";
                    objInput.Data = null;
                }

                else
                {

                    List<CardsTbl> objCardsData = _dbContext.CardsTbl.Where(p => p.UserId == UserId).ToList();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCardsData;
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
        [Route("Card/Get/Default")]
        public dynamic Card_Get()
        {
            InputData objInput = new InputData();
            try
            {
                int UserId = 2;
                if (!_dbContext.CardsTbl.Where(p => p.UserId == UserId && p.IsDefault == true).Any())
                {
                    objInput.success = false;
                    objInput.message = "Cards is ont Exist!!";
                    objInput.Data = null;
                }
                else
                {
                    CardsTbl objCardsData = _dbContext.CardsTbl.Where(p => p.UserId == UserId &&  p.IsDefault == true).FirstOrDefault();

                    //DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                    //String xToken = objCardsData.Id.ToString() + "#" + objCardsData.UserName + "#" + tomorrow.ToString();
                    //objInput.AuthToken = Encrypt(xToken);
                    objInput.success = true;
                    objInput.message = "Successfully. ";
                    objInput.Data = objCardsData;
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
        [Route("Address/Insert")]
        public dynamic Address_Insert([FromBody]AddressTbl objAddresssData)
        {
            InputData objInput = new InputData();
            try
            {
                if (false)
                {
                    objInput.success = false;
                    objInput.message = "Addresss is already register!!";
                    objInput.Data = null;
                }
                else
                {

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
        [Route("Address/Edit/{AddressId}")]
        public dynamic Address_Edit([FromBody]AddressTbl objAddresssData, int AddressId)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.AddressTbl.Where(p => p.Id == AddressId).Any())
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
                    dbobjAddresss.Mobile = objAddresssData.Mobile == 0 ? dbobjAddresss.Mobile : objAddresssData.Mobile;
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
                if (_dbContext.AddressTbl.Where(p => p.Id == AddressId).Any())
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
                    _dbContext.AddressTbl.Remove(objAddresssData);

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
                int UserId = 2;
                if (!_dbContext.AddressTbl.Where(p => p.UserId == UserId && p.IsDeleted == false).Any())
                {
                    objInput.success = false;
                    objInput.message = "Addresss is ont Exist!!";
                    objInput.Data = null;
                }

                else
                {

                    List<AddressTbl> objAddresssData = _dbContext.AddressTbl.Where(p => p.UserId == UserId && p.IsDeleted == false).ToList();

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
                else if (_dbContext.AddressTbl.Where(p => p.Id == AddressId).Any())
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
                        if(add.Id == AddressId)
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
            InputData objInput = new InputData();
            try
            {
                //AdminMstr UpdateObj = _dbContext.AdminMstr.Single(p => p.ContactNumber == objAdminData.ContactNumber);
                //UpdateObj.Token = objUserData.noti_token;
                //_SellerProductContext.SaveChanges();

                var Data = _dbContext.AdminMstr.Where(p => (p.UserName == objAdminData.UserName || p.ContactNumber == Decimal.Parse(objAdminData.UserName)) && p.Password == objAdminData.Password).ToList();

                if (Data.Count == 0)
                {
                    objInput.success = false;
                    objInput.message = "Incorrect Password.";
                    objInput.Data = null;
                }
                else
                {
                    if (Data[0].IsActive)
                    {
                        DateTime tomorrow = DateTime.UtcNow.AddHours(24);
                        string xToken = Data[0].Id.ToString() + "#" + Data[0].UserName + "#" + tomorrow.ToString();

                        objInput.success = true;
                        objInput.message = "Successfully.";
                        objInput.Data = Data;
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
                }else if (_dbContext.CategoryMstr.Where(p => p.Name == objCategoryData.Name).Any())
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
                    List<CategoryMstr> dbCategory = _dbContext.CategoryMstr.ToList();

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
        [Route("Coupon/Insert")]
        public dynamic Coupon_Insert([FromBody]CouponMstr objCouponData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.CouponMstr.Where(p => p.CoupCode == objCouponData.CoupCode).Any())
                {
                    objInput.success = false;
                    objInput.message = "Coupon code is already register!!";
                    objInput.Data = null;
                }
                else
                {
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
                else if (_dbContext.CouponMstr.Where(p => p.CoupCode == objCouponData.CoupCode).Any())
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



        #endregion


        #region vender Side Area

        #region vender CRUD z

        [HttpPost]
        [Route("Vender/Insert")]
        public dynamic Vender_Insert([FromBody]VenderMstr objVenderData)
        {
            InputData objInput = new InputData();
            try
            {
                if (_dbContext.VenderMstr.Where(p => p.MobileNumber == objVenderData.MobileNumber).Any())
                {
                    objInput.success = false;
                    objInput.message = "Vender MobileNumber is already register!!";
                    objInput.Data = null;
                }
                else
                {
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
                    newproduct.VenderId = CustProductData.VenderId;
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


                    foreach (CostSubProduct subProduct in CustProductData.SubProductTbl.ToList())
                    {
                        SubProductTbl newsubProduct = new SubProductTbl();
                        newsubProduct.ProductId = newproduct.Id;
                        newsubProduct.SizeId = subProduct.SizeId;
                        newsubProduct.ColorId = subProduct.ColorId;
                        newsubProduct.Price = subProduct.Price;
                        newsubProduct.OfferPrice = subProduct.OfferPrice;
                        newsubProduct.Qty = subProduct.Qty;

                        newsubProduct.IsActive = true;
                        newsubProduct.IsDeleted = false;
                        newsubProduct.CreateDt = DateTime.UtcNow;
                        newsubProduct.UpdateDt = DateTime.UtcNow;
                        _dbContext.SubProductTbl.Add(newsubProduct);
                        _dbContext.SaveChanges();

                        foreach (CustProductImg pimg in subProduct.ProductImg.ToList())
                        {
                            ProductImg newProImg = new ProductImg();
                            newProImg.SubProducatId = newsubProduct.Id;
                            newProImg.Path = pimg.Path;
                            newProImg.IsActive = true;
                            newProImg.IsDeleted = false;
                            newProImg.CreateDt = DateTime.UtcNow;
                            newProImg.UpdateDt = DateTime.UtcNow;
                            _dbContext.ProductImg.Add(newProImg);
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


                foreach (CostSubProduct subProduct in CustProductData.SubProductTbl.ToList())
                {
                    SubProductTbl newsubProduct = _dbContext.SubProductTbl.Find(subProduct.Id);
                    newsubProduct.ProductId = newproduct.Id;
                    newsubProduct.SizeId = subProduct.SizeId;
                    newsubProduct.ColorId = subProduct.ColorId;
                    newsubProduct.Price = subProduct.Price;
                    newsubProduct.OfferPrice = subProduct.OfferPrice;
                    newsubProduct.Qty = subProduct.Qty;

                    newsubProduct.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    foreach (CustProductImg pimg in subProduct.ProductImg.ToList())
                    {
                        ProductImg newProImg = _dbContext.ProductImg.Find(pimg.Id);
                        newProImg.SubProducatId = newsubProduct.Id;
                        newProImg.Path = pimg.Path;
                        newProImg.UpdateDt = DateTime.UtcNow;
                        _dbContext.SaveChanges();
                    }

                }


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
                    newproduct.IsActive = false;
                    newproduct.IsDeleted = true;
                    newproduct.UpdateDt = DateTime.UtcNow;
                    _dbContext.SaveChanges();


                    foreach (SubProductTbl subProduct in _dbContext.SubProductTbl.Where(p => p.ProductId == newproduct.Id).ToList())
                    {

                        subProduct.IsActive = false;
                        subProduct.IsDeleted = true;
                        subProduct.UpdateDt = DateTime.UtcNow;
                        _dbContext.SaveChanges();

                        foreach (ProductImg pimg in _dbContext.ProductImg.Where(p => p.SubProducatId == subProduct.Id).ToList())
                        {
                            pimg.IsActive = false;
                            pimg.IsDeleted = true;
                            pimg.UpdateDt = DateTime.UtcNow;
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

                    foreach (ProductMstr newproduct in productList)
                    {
                        foreach (SubProductTbl subProduct in _dbContext.SubProductTbl.Where(p => p.ProductId == newproduct.Id).ToList())
                        {
                            foreach (ProductImg pimg in _dbContext.ProductImg.Where(p => p.SubProducatId == subProduct.Id).ToList())
                            {
                                subProduct.ProductImg.Add(pimg);
                            }
                            newproduct.SubProductTbl.Add(subProduct);
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
        [Route("Product/GetAll/{VenderId}")]
        public dynamic Product_GetAll(int VenderId)
        {
            InputData objInput = new InputData();
            try
            {
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

                    foreach (ProductMstr newproduct in productList)
                    {
                        foreach (SubProductTbl subProduct in _dbContext.SubProductTbl.Where(p => p.ProductId == newproduct.Id).ToList())
                        {
                            foreach (ProductImg pimg in _dbContext.ProductImg.Where(p => p.SubProducatId == subProduct.Id).ToList())
                            {
                                subProduct.ProductImg.Add(pimg);
                            }
                            newproduct.SubProductTbl.Add(subProduct);
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

        public class CustProduct
        {

            public int Id { get; set; }
            public int CatId { get; set; }
            public int VenderId { get; set; }
            public string Sku { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Tags { get; set; }
            public bool IsReturnable { get; set; }
            public decimal ReturnDays { get; set; }
            public string Policy { get; set; }
            public double CurrentRating { get; set; }
            public int RatingCount { get; set; }
            public int ReviewCount { get; set; }
            public bool UserListing { get; set; }
            public decimal PackWeight { get; set; }
            public decimal PackLenght { get; set; }
            public decimal PackBreadth { get; set; }
            public decimal PackHeight { get; set; }

            public ICollection<CostSubProduct> SubProductTbl { get; set; }
        }
        public class CostSubProduct
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int SizeId { get; set; }
            public int ColorId { get; set; }
            public decimal Price { get; set; }
            public decimal OfferPrice { get; set; }
            public decimal Qty { get; set; }
            public ICollection<CustProductImg> ProductImg { get; set; }
        }
        public class CustProductImg
        {
            public int Id { get; set; }
            public int SubProducatId { get; set; }
            public string Path { get; set; }
        }
    }

}
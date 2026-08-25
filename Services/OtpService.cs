using Capsitech;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Models;
using System.Security.Cryptography;
using System.Text;

namespace Projects.Services
{
    public class OtpService
    {
        private readonly IMongoCollection<OtpModel> _otp;
        private readonly IEmailSender _emailSender;

        public OtpService(IOptions<DbSettings> dbsettings, IEmailSender emailSender)
        {
            var mongoclient = new MongoClient(dbsettings.Value.ConnectionString);
            var dataBase = mongoclient.GetDatabase(dbsettings.Value.DatabaseName);
            _otp = dataBase.GetCollection<OtpModel>(DbCollections.Otps);
            _emailSender = emailSender;
        }

        public async Task<string> GenerateOtpAsync(string userId, string email)
        {
            // Invalidate previous OTPs
            var oldOtpFilter = Builders<OtpModel>.Filter.And(
                Builders<OtpModel>.Filter.Eq(x => x.UserId, userId),
                Builders<OtpModel>.Filter.Eq(x => x.IsUsed, false)
            );

            var invalidateUpdate = Builders<OtpModel>.Update
                .Set(x => x.IsUsed, true);

            await _otp.UpdateManyAsync(
                oldOtpFilter,
                invalidateUpdate
            );


            // Generate OTP
            string otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();


            // Generate temporary session
            string sessionId = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32)
            );


            string otpHash = HashOtp(otp);


            var otpModel = new OtpModel
            {
                UserId = userId,
                Email = email,
                SessionId = sessionId,
                OtpHash = otpHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };


            await _otp.InsertOneAsync(otpModel);


            await _emailSender.SendEmailAsync(
                email,
                "Your OTP",
                $"Your OTP is {otp}. It will expire in 5 minutes."
            );


            return sessionId;
        }

        public async Task<string> VerifyOtpAsync(string sessionId, string otp)
        {
            var otpHash = HashOtp(otp);

            var filter = Builders<OtpModel>.Filter.And(
                Builders<OtpModel>.Filter.Eq(x => x.SessionId, sessionId),
                Builders<OtpModel>.Filter.Eq(x => x.OtpHash, otpHash),
                Builders<OtpModel>.Filter.Eq(x => x.IsUsed, false)
            );

            var otpRecord = await _otp
                .Find(filter)
                .SortByDescending(x => x.ExpiresAt)
                .FirstOrDefaultAsync();

            // OTP doesn't exist
            if (otpRecord == null) throw new AppModelException("otp recoder does not found");

            // OTP expired
            if (otpRecord.ExpiresAt <= DateTime.UtcNow) throw new AppModelException("Invalid OTP");

            // Mark OTP as used
            var update = Builders<OtpModel>.Update
                .Set(x => x.IsUsed, true);

            await _otp.UpdateOneAsync(
                Builders<OtpModel>.Filter.Eq(x => x.Id, otpRecord.Id),
                update
            );

            return otpRecord.UserId;
        }

        private static string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();

            return Convert.ToHexString(
                sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(otp)
                )
            );
        }
    }
}

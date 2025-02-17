using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir_Logging;
using UniversalEsir_SportSchedulerAPI.RequestModel.Obavestenja;
using UniversalEsir_SportSchedulerAPI.RequestModel.Racun;
using UniversalEsir_SportSchedulerAPI.RequestModel.Uplata;
using UniversalEsir_SportSchedulerAPI.RequestModel.User;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Racuni;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Uplate;
using UniversalEsir_SportSchedulerAPI.ResponseModel.User;

namespace UniversalEsir_SportSchedulerAPI
{
    public class SportSchedulerAPI_Manager
    {
        #region Fields
#if DEBUG
        private string _url = @"https://localhost:44465";
#else
        private string _url = @"https://tksirmium.com";
#endif
        #endregion Fields

        #region Constructors
        public SportSchedulerAPI_Manager()
        {
        }
        #endregion Constructors

        #region Properties internal
        #endregion Properties internal

        #region Properties

        #endregion Properties

        #region Commands
        #endregion Commands

        #region Public methods
        #region Users
        public async Task<IEnumerable<UserResponse>?> GetUsersAsync()
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/Users/getAllUsersFromKlub/1";

                var response = await client.GetAsync(requestUrl).ConfigureAwait(false);
                //var response = client.GetAsync(requestUrl).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> GetUsersAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return null;
                }

                try
                {
                    // Ovdje obradite uspešan odgovor
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var users = JsonConvert.DeserializeObject<IEnumerable<UserResponse>>(responseData);

                    return users;
                }
                catch (Exception ex)
                {
                    Log.Error("SportSchedulerAPI_Manager -> GetUsersAsync -> Odgovor nije dobar: ", ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> GetUsersAsync -> Greska prilikom GetUsers: ", ex);
                return null;
            }
        }
        public async Task<bool> PostUsersAsync(UserRequest userRequest)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/Users";

                var response = await client.PostAsJsonAsync(requestUrl, userRequest).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> PostUsersAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> PostUsersAsync -> Greska prilikom PostUser: ", ex);
                return false;
            }
        }
        public async Task<bool> PutUsersAsync(UserRequest userRequest)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/Users/{userRequest.Id}";

                var response = await client.PutAsJsonAsync(requestUrl, userRequest).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> PutUsersAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return false;
                }

                try
                {
                    // Ovdje obradite uspešan odgovor
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var user = JsonConvert.DeserializeObject<UserResponse>(responseData);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error("SportSchedulerAPI_Manager -> PutUsersAsync -> Odgovor nije dobar: ", ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> PutUsersAsync -> Greska prilikom PutUser: ", ex);
                return false;
            }
        }
        #endregion Users

        #region Uplate
        public async Task<IEnumerable<UplataResponse>?> GetUplateAsync(int idUser)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/all/Uplatas/{idUser}";

                var response = await client.GetAsync(requestUrl).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> GetUplateAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return null;
                }

                try
                {
                    // Ovdje obradite uspešan odgovor
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var uplate = JsonConvert.DeserializeObject<IEnumerable<UplataResponse>>(responseData);

                    return uplate;
                }
                catch (Exception ex)
                {
                    Log.Error("SportSchedulerAPI_Manager -> GetUplateAsync -> Odgovor nije dobar: ", ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> GetUplateAsync -> Greska prilikom GetUplateAsync: ", ex);
                return null;
            }
        }
        public async Task<bool> PostUplataAsync(UplataRequest uplataRequest)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/Uplatas";

                var response = await client.PostAsJsonAsync(requestUrl, uplataRequest).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> PostUplataAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> PostUplataAsync -> Greska prilikom PostUplataAsync: ", ex);
                return false;
            }
        }
        #endregion Uplate

        #region Obavestenja
        public async Task<bool> PostObavestenjeAsync(ObavestenjeRequest obavestenjeRequest)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/Obavestenjas";

                var response = await client.PostAsJsonAsync(requestUrl, obavestenjeRequest).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> PostObavestenjeAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> PostObavestenjeAsync -> Greska prilikom PostObavestenjeAsync: ", ex);
                return false;
            }
        }
        #endregion Obavestenja

        #region Racuni
        public async Task<IEnumerable<RacunResponse>?> GetRacunsAsync(int idUser)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/Racuns/all/{idUser}";

                var response = await client.GetAsync(requestUrl).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> GetRacunsAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return null;
                }

                try
                {
                    // Ovdje obradite uspešan odgovor
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var racuni = JsonConvert.DeserializeObject<IEnumerable<RacunResponse>>(responseData);

                    return racuni;
                }
                catch (Exception ex)
                {
                    Log.Error("SportSchedulerAPI_Manager -> GetRacunsAsync -> Odgovor nije dobar: ", ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> GetRacunsAsync -> Greska prilikom GetRacunsAsync: ", ex);
                return null;
            }
        }
        public async Task<bool> PostRacunAsync(RacunRequest racunRequest)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                string requestUrl = $"{_url}/api/Racuns";

                var response = await client.PostAsJsonAsync(requestUrl, racunRequest).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"SportSchedulerAPI_Manager -> PostRacunAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("SportSchedulerAPI_Manager -> PostRacunAsync -> Greska prilikom PostUser: ", ex);
                return false;
            }
        }
        #endregion Racuni

        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}



using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using exam.Models;
using NUnit.Framework;



namespace exam

{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string createdMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI2ZDk0YzRjZS05YzdkLTRjY2ItYjk5My0wZTBiMjMzNGU4ODAiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjIzOjUwIiwiVXNlcklkIjoiMWMzYWUzZjUtMGFkOS00MDdjLTYyMWMtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJ0b3ZhQGFidi5iZyIsIlVzZXJOYW1lIjoidG92YTEiLCJleHAiOjE3NzY1MTUwMzAsImlzcyI6Ik1vdmllQ2F0YWxvZ19BcHBfU29mdFVuaSIsImF1ZCI6Ik1vdmllQ2F0YWxvZ19XZWJBUElfU29mdFVuaSJ9.NQr_wRIsy_Bn79Yo8MP79XX7_X9WZ1aUm4aM0DgyMI0";
        private const string LoginEmail = "tova@abv.bg";
        private const string LoginPassword = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateMovie_WithRequiredFIelds_ShouldReturnSuccess()
        {
            var movieData = new MovieDTO
            {
                Title = "Test movie",
                Description = "This is a test movie description.",
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this.client.Execute(request);

            var result = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Movie, Is.Not.Null);
            Assert.That(result.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Msg, Is.EqualTo("Movie created successfully!"));

            createdMovieId = result.Movie.Id;

        }

        [Order(2)]
        [Test]

        public void EditExistingMovie_ShouldReturnSuccess()
        {

            var request = new RestRequest("/api/Movie/Edit/", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);

            var body = new
            {
        
            title = "Test movie edited",
            description = "This is a test movie description edited.",
           

        };

            request.AddJsonBody(body);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var result = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(result.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");

            var responseItems = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);

            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);
        }



        [Order(4)]
        [Test]

        public void DeleteMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);
            var response = this.client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            var responseItems = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseItems.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var movieData = new MovieDTO
            {
                Title = "",
                Description = "",
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(6)]
        [Test]

        public void EditNonExistingMovie_ShouldReturnNotFound()
        {
            string nonExistingMovieId = "9999999";

            var body = new
            {

                title = "Test movie not exising",
                description = "This is a test movie description not exising.",


            };
            var request = new RestRequest("/api/Movie/Edit/", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(body);

            var response = this.client.Execute(request);
            var responseItems = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 BadRequest.");
            Assert.That(responseItems.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnNotFound()
        {

            string nonExistingMovieId = "12345";

            var request = new RestRequest("/api/Movie/Delete/", Method.Delete);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var result = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(result?.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));

        }


        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using System.IO;
using Newtonsoft.Json.Linq;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms;
using System.Windows.Forms;
using POGOProtos.Map.Fort;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Map : BaseRpc
    {
        public List<KeyValuePair<int, GMap.NET.PointLatLng>> CaughtMarkers = new List<KeyValuePair<int, GMap.NET.PointLatLng>>();

        private Client _client;

        public Map(Client client) : base(client)
        {
            _client = client;
        }

        public Tuple<double, double> GetLatLngFromFile()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\Configs\\Coords.ini") &&
                File.ReadAllText(Directory.GetCurrentDirectory() + "\\Configs\\Coords.ini").Contains(":"))
            {
                var latlngFromFile = File.ReadAllText(Directory.GetCurrentDirectory() + "\\Configs\\Coords.ini");
                var latlng = latlngFromFile.Split(':');
                if (latlng[0].Length != 0 && latlng[1].Length != 0)
                {
                    try
                    {
                        double temp_lat = Convert.ToDouble(latlng[0]);
                        double temp_long = Convert.ToDouble(latlng[1]);

                        if (temp_lat >= -90 && temp_lat <= 90 && temp_long >= -180 && temp_long <= 180)
                        {
                            return new Tuple<double, double>(temp_lat, temp_long);
                        }
                        else
                        {
                            Logger.Write("Coordinates in \"Coords.ini\" file are invalid, using the default coordinates ",
                            LogLevel.Warning);
                            return null;
                        }
                    }
                    catch (FormatException)
                    {
                        Logger.Write("Coordinates in \"Coords.ini\" file are invalid, using the default coordinates ",
                            LogLevel.Warning);
                        return null;
                    }
                }

            }

            return null;
        }

        public async Task<GetMapObjectsResponse> GetMapObjects()
        {
            #region Messages

            var getMapObjectsMessage = new GetMapObjectsMessage
            {
                CellId = { S2Helper.GetNearbyCellIds(_client.CurrentLongitude, _client.CurrentLatitude) },
                SinceTimestampMs = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Latitude = _client.CurrentLatitude,
                Longitude = _client.CurrentLongitude
            };
            var getHatchedEggsMessage = new GetHatchedEggsMessage();
            var getInventoryMessage = new GetInventoryMessage
            {
                LastTimestampMs = DateTime.UtcNow.ToUnixTime()
            };
            var checkAwardedBadgesMessage = new CheckAwardedBadgesMessage();
            var downloadSettingsMessage = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            #endregion

            var request = RequestBuilder.GetRequestEnvelope(
                new Request
                {
                    RequestType = RequestType.GetMapObjects,
                    RequestMessage = getMapObjectsMessage.ToByteString()
                },
                new Request
                {
                    RequestType = RequestType.GetHatchedEggs,
                    RequestMessage = getHatchedEggsMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetInventory,
                    RequestMessage = getInventoryMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.CheckAwardedBadges,
                    RequestMessage = checkAwardedBadgesMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.DownloadSettings,
                    RequestMessage = downloadSettingsMessage.ToByteString()
                });

            var response = await PostProtoPayload<Request, GetMapObjectsResponse>(request);

            lock (Client._lock)
            {
                _client._map.Invoke(new MethodInvoker(delegate
                {
                    var mapPointOverlay = _client._map.Overlays[2];
                    mapPointOverlay.Markers.Clear();
                    var pokemonOverlay = _client._map.Overlays[3];


                    foreach (var mapPoint in response.MapCells)
                    {
                        if (_client.Settings.PurePokemonMode)
                        {
                            foreach (var spawn in mapPoint.SpawnPoints)
                            {
                                mapPointOverlay.Markers.Add(new GMarkerGoogle(new GMap.NET.PointLatLng(spawn.Latitude, spawn.Longitude),
                        GMarkerGoogleType.brown_small));
                            }
                        }
                        else
                        {
                            foreach (var fort in mapPoint.Forts)
                            {
                                if (fort.Type == FortType.Checkpoint)
                                {
                                    mapPointOverlay.Markers.Add(new GMarkerGoogle(new GMap.NET.PointLatLng(fort.Latitude, fort.Longitude),
                            GMarkerGoogleType.blue_small));
                                }
                            }
                        }


                        GetPokemonFromPokeVision(pokemonOverlay);

                        //foreach (var pokemon in mapPoint.WildPokemons)
                        //{
                        //    pokemonOverlay.Markers.Add(new GMarkerGoogle(new GMap.NET.PointLatLng(pokemon.Latitude, pokemon.Longitude),
                        //Images.GetPokemonImage((int)pokemon.PokemonData.PokemonId)));
                        //}

                    }
                }));
            }

            return response;
        }

        public async Task<GetIncensePokemonResponse> GetIncensePokemons()
        {
            var message = new GetIncensePokemonMessage()
            {
                PlayerLatitude = _client.CurrentLatitude,
                PlayerLongitude = _client.CurrentLongitude
            };

            return await PostProtoPayload<Request, GetIncensePokemonResponse>(RequestType.GetIncensePokemon, message);
        }

        public async void GetPokemonFromPokeVision(GMapOverlay overlay)
        {
            try
            {
                var request = await _client.PokemonHttpClient.GetAsync("https://pokevision.com/map/scan/" + _client.CurrentLatitude + "/" + _client.CurrentLongitude);
                var response = request.Content.ReadAsStringAsync().Result;

                var parser = JObject.Parse(response);

                if (parser["status"].ToString() != "success")
                {
                    return;
                }

                var result = new JObject();

                do
                {

                    var jobId = parser["jobId"];
                    var dataUrl = "https://pokevision.com/map/data/" + _client.CurrentLatitude + "/" + _client.CurrentLongitude + "/" + jobId;

                    var data = await _client.PokemonHttpClient.GetAsync(dataUrl);
                    var dataresponse = data.Content.ReadAsStringAsync().Result;

                    result = JObject.Parse(dataresponse);

                } while (result["jobStatus"]?.ToString() == "in_progress");


                JArray pokemon = (JArray)result["pokemon"];

                overlay.Markers.Clear();

                foreach (var item in pokemon)
                {
                    var position = new GMap.NET.PointLatLng(Math.Round((double)item["latitude"], 12), Math.Round((double)item["longitude"], 12));

                    var pokemonId = (int)item["pokemonId"];
                    if (CaughtMarkers.Contains(new KeyValuePair<int, GMap.NET.PointLatLng>((int)item["pokemonId"], position)))
                    {
                        continue;
                    }

                    overlay.Markers.Add(new GMarkerGoogle(position,
                    Images.GetPokemonImage(pokemonId)));

                }

            }
            catch (Exception ex)
            {

                // throw;
            }
        }
    }
}

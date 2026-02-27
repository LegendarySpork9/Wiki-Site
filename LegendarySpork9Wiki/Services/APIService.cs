using LegendarySpork9Wiki.Models;
using Newtonsoft.Json;
using RestSharp;

namespace LegendarySpork9Wiki.Services
{
    public class APIService
    {
        private readonly SharedSettingsModel _settings;
        private readonly LoggerService _logger;
        private string _bearerToken = string.Empty;

        private bool UseDummyData => string.IsNullOrEmpty(_settings.BaseURL);

        public APIService(SharedSettingsModel settings, LoggerService logger)
        {
            _settings = settings;
            _logger = logger;
        }

        #region API Methods

        private async Task<string> GetBearerTokenAsync()
        {
            if (UseDummyData) return string.Empty;

            try
            {
                string payloadPath = Path.Combine(_settings.PayloadLocation, "Authorise.json");
                string payload = File.ReadAllText(payloadPath);

                using var client = new RestClient(_settings.BaseURL);
                var request = new RestRequest("auth/token", Method.Post);
                request.AddStringBody(payload, DataFormat.Json);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    dynamic? result = JsonConvert.DeserializeObject(response.Content);
                    _bearerToken = result?.token ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get bearer token", ex);
            }

            return _bearerToken;
        }

        private async Task<string?> MakeRequestAsync(string endpoint, Method method = Method.Get, string? body = null)
        {
            if (UseDummyData) return null;

            try
            {
                if (string.IsNullOrEmpty(_bearerToken))
                {
                    await GetBearerTokenAsync();
                }

                using var client = new RestClient(_settings.BaseURL);
                var request = new RestRequest(endpoint, method);
                request.AddHeader("Authorization", $"Bearer {_bearerToken}");

                if (!string.IsNullOrEmpty(body))
                {
                    request.AddStringBody(body, DataFormat.Json);
                }

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await GetBearerTokenAsync();
                    request.AddOrUpdateHeader("Authorization", $"Bearer {_bearerToken}");
                    response = await client.ExecuteAsync(request);
                }

                if (response.IsSuccessful)
                {
                    return response.Content;
                }

                _logger.LogWarning($"API request to {endpoint} returned {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"API request to {endpoint} failed", ex);
            }

            return null;
        }

        #endregion

        #region Auth

        public async Task<UserModel?> AuthoriseAsync(string username, string passwordHash)
        {
            if (UseDummyData)
            {
                return GetDummyUser(username, passwordHash);
            }

            try
            {
                string payloadPath = Path.Combine(_settings.PayloadLocation, "Authorise.json");
                string payload = File.ReadAllText(payloadPath)
                    .Replace("{username}", username)
                    .Replace("{password}", passwordHash);

                string? response = await MakeRequestAsync("users/authorise", Method.Post, payload);

                if (!string.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<UserModel>(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Authorise failed", ex);
            }

            return null;
        }

        #endregion

        #region Users

        public async Task<UserModel?> GetUserSettings(string userId)
        {
            if (UseDummyData) return GetDummyUserSettings();

            string? response = await MakeRequestAsync($"users/{userId}/settings");

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<UserModel>(response);
            }

            return null;
        }

        public async Task<bool> UpdateUser(string userId, string username, string passwordHash)
        {
            if (UseDummyData) return true;

            try
            {
                string payloadPath = Path.Combine(_settings.PayloadLocation, "UpdateUser.json");
                string payload = File.ReadAllText(payloadPath)
                    .Replace("{username}", username)
                    .Replace("{password}", passwordHash);

                string? response = await MakeRequestAsync($"users/{userId}", Method.Put, payload);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateUser failed", ex);
                return false;
            }
        }

        public async Task<bool> UpdateUserSettings(string userId, bool darkMode)
        {
            if (UseDummyData) return true;

            try
            {
                string payloadPath = Path.Combine(_settings.PayloadLocation, "UpdateUserSettings.json");
                string payload = File.ReadAllText(payloadPath)
                    .Replace("{darkMode}", darkMode.ToString().ToLower());

                string? response = await MakeRequestAsync($"users/{userId}/settings", Method.Put, payload);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateUserSettings failed", ex);
                return false;
            }
        }

        #endregion

        #region Categories

        public async Task<List<CategoryModel>> GetCategories()
        {
            if (UseDummyData) return GetDummyCategories();

            string? response = await MakeRequestAsync("categories");

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<List<CategoryModel>>(response) ?? new List<CategoryModel>();
            }

            return new List<CategoryModel>();
        }

        public async Task<CategoryModel?> CreateCategory(string name, string description, string icon, string? parentCategoryId)
        {
            if (UseDummyData) return new CategoryModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Icon = icon,
                ParentCategoryId = parentCategoryId
            };

            try
            {
                string payloadPath = Path.Combine(_settings.PayloadLocation, "CreateCategory.json");
                string payload = File.ReadAllText(payloadPath)
                    .Replace("{name}", name)
                    .Replace("{description}", description)
                    .Replace("{icon}", icon)
                    .Replace("{parentCategoryId}", parentCategoryId ?? "");

                string? response = await MakeRequestAsync("categories", Method.Post, payload);

                if (!string.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<CategoryModel>(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateCategory failed", ex);
            }

            return null;
        }

        #endregion

        #region Entries

        public async Task<APIEntriesModel> GetEntries(string categoryId, int pageNumber = 1)
        {
            if (UseDummyData) return GetDummyEntries(categoryId);

            string? response = await MakeRequestAsync($"entries?categoryId={categoryId}&page={pageNumber}");

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<APIEntriesModel>(response) ?? new APIEntriesModel { APICalled = true };
            }

            return new APIEntriesModel { APICalled = true };
        }

        public async Task<EntryModel?> GetEntry(string entryId)
        {
            if (UseDummyData) return GetDummyEntry(entryId);

            string? response = await MakeRequestAsync($"entries/{entryId}");

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<EntryModel>(response);
            }

            return null;
        }

        public async Task<List<EntryModel>> GetRecentEntries()
        {
            if (UseDummyData) return GetDummyRecentEntries();

            string? response = await MakeRequestAsync("entries/recent");

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<List<EntryModel>>(response) ?? new List<EntryModel>();
            }

            return new List<EntryModel>();
        }

        public async Task<List<EntryModel>> SearchEntries(string query)
        {
            if (UseDummyData) return GetDummyRecentEntries().Where(e => e.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

            string? response = await MakeRequestAsync($"entries/search?q={Uri.EscapeDataString(query)}");

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<List<EntryModel>>(response) ?? new List<EntryModel>();
            }

            return new List<EntryModel>();
        }

        public async Task<EntryModel?> CreateEntry(APINewEntryModel entry)
        {
            if (UseDummyData) return new EntryModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = entry.Title,
                Content = entry.Content,
                CategoryId = entry.CategoryId,
                Author = entry.Author,
                Summary = entry.Summary,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now
            };

            try
            {
                string payloadPath = Path.Combine(_settings.PayloadLocation, "CreateEntry.json");
                string payload = File.ReadAllText(payloadPath)
                    .Replace("{title}", entry.Title)
                    .Replace("{content}", entry.Content)
                    .Replace("{categoryId}", entry.CategoryId)
                    .Replace("{author}", entry.Author)
                    .Replace("{summary}", entry.Summary);

                string? response = await MakeRequestAsync("entries", Method.Post, payload);

                if (!string.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<EntryModel>(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateEntry failed", ex);
            }

            return null;
        }

        public async Task<bool> UpdateEntry(string entryId, APINewEntryModel entry)
        {
            if (UseDummyData) return true;

            try
            {
                string payloadPath = Path.Combine(_settings.PayloadLocation, "UpdateEntry.json");
                string payload = File.ReadAllText(payloadPath)
                    .Replace("{title}", entry.Title)
                    .Replace("{content}", entry.Content)
                    .Replace("{categoryId}", entry.CategoryId)
                    .Replace("{author}", entry.Author)
                    .Replace("{summary}", entry.Summary);

                string? response = await MakeRequestAsync($"entries/{entryId}", Method.Put, payload);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateEntry failed", ex);
                return false;
            }
        }

        #endregion

        #region Dummy Data

        private static readonly string DummyAdminPasswordHash = Functions.HashFunction.ComputeSHA512("admin");

        private UserModel? GetDummyUser(string username, string passwordHash)
        {
            if (username == "admin" && passwordHash == DummyAdminPasswordHash)
            {
                return new UserModel
                {
                    UserId = "1",
                    Username = "admin",
                    Admin = true,
                    DarkMode = false
                };
            }

            return null;
        }

        private UserModel GetDummyUserSettings()
        {
            return new UserModel
            {
                UserId = "1",
                Username = "admin",
                Admin = true,
                DarkMode = false
            };
        }

        private List<CategoryModel> GetDummyCategories()
        {
            return new List<CategoryModel>
            {
                new CategoryModel
                {
                    Id = "cat-1",
                    Name = "Cold Waters",
                    Description = "A game made by Killerfish Games in which you are in control of a submarine during a range of scenarios.",
                    Icon = "bi-water",
                    SubCategories = new List<CategoryModel>
                    {
                        new CategoryModel
                        {
                            Id = "cat-1-1",
                            Name = "South China Sea 2000 NATO",
                            Description = "The South China Sea 2000 NATO campaign scenario.",
                            Icon = "bi-globe-americas",
                            ParentCategoryId = "cat-1",
                            SubCategories = new List<CategoryModel>
                            {
                                new CategoryModel
                                {
                                    Id = "cat-1-1-1",
                                    Name = "CDR Toby Hunter",
                                    Description = "Character lore for Commander Toby Hunter.",
                                    Icon = "bi-person-badge",
                                    ParentCategoryId = "cat-1-1"
                                }
                            }
                        }
                    }
                },
                new CategoryModel
                {
                    Id = "cat-2",
                    Name = "The Long Dark",
                    Description = "A survival game by Hinterland Studios. You play as either Will Mackenzie or Astrid Greenwood, trying to survive in a hostile environment.",
                    Icon = "bi-snow",
                    SubCategories = new List<CategoryModel>
                    {
                        new CategoryModel
                        {
                            Id = "cat-2-1",
                            Name = "Will Mackenzie (Stalker)",
                            Description = "Following the journey of Will Mackenzie on Stalker difficulty.",
                            Icon = "bi-person-walking",
                            ParentCategoryId = "cat-2"
                        }
                    }
                }
            };
        }

        private List<EntryModel> GetAllDummyEntries()
        {
            return new List<EntryModel>
            {
                new EntryModel
                {
                    Id = "entry-1",
                    Title = "CDR Toby Hunter — Background",
                    Content = @"<h2>Early Life</h2>
<img src=""https://placehold.co/800x300/1a1a2e/deepskyblue?text=USS+Cheyenne+%28SSN-773%29"" alt=""USS Cheyenne"" class=""article-image"" />
<p>Commander Toby Hunter was born in Annapolis, Maryland, to a family with a long tradition of naval service. His father, Captain Richard Hunter, served aboard USS <em>Dallas</em> during the latter years of the Cold War, and his grandfather saw action in the Pacific during World War II.</p>

<h2>Naval Career</h2>
<p>Hunter graduated from the United States Naval Academy in 1988, ranking in the top ten percent of his class. He was immediately drawn to the submarine service, volunteering for the nuclear submarine programme.</p>
<img src=""https://placehold.co/400x250/16213e/87ceeb?text=Naval+Academy+%2788"" alt=""Naval Academy graduation"" class=""article-image article-image-right"" />
<p>His first posting was aboard USS <em>Los Angeles</em> (SSN-688) as a junior officer, where he quickly distinguished himself during operations in the North Atlantic. His commanding officer noted his exceptional situational awareness and calm demeanour under pressure.</p>

<h2>Command</h2>
<p>By 2000, Hunter had risen to the rank of Commander and was given command of USS <em>Cheyenne</em> (SSN-773), a <em>Los Angeles</em>-class submarine. It was aboard <em>Cheyenne</em> that he would face his greatest challenge during the South China Sea crisis.</p>

<h2>Personality</h2>
<p>Known among his crew as measured and methodical, Hunter rarely raised his voice. He earned loyalty through competence rather than charisma, and was respected for never asking his crew to take risks he would not take himself.</p>",
                    CategoryId = "cat-1-1-1",
                    CategoryName = "CDR Toby Hunter",
                    Author = "admin",
                    DateCreated = new DateTime(2024, 6, 15),
                    DateModified = new DateTime(2024, 8, 20),
                    Summary = "Background and early career of Commander Toby Hunter, commanding officer of USS Cheyenne during the South China Sea 2000 campaign.",
                    ThumbnailUrl = "https://placehold.co/400x200/1a1a2e/deepskyblue?text=CDR+Toby+Hunter"
                },
                new EntryModel
                {
                    Id = "entry-2",
                    Title = "South China Sea 2000 — Campaign Overview",
                    Content = @"<h2>Setting</h2>
<img src=""https://placehold.co/800x300/16213e/87ceeb?text=South+China+Sea+Theatre"" alt=""South China Sea theatre of operations"" class=""article-image"" />
<p>The year is 2000. Tensions in the South China Sea have reached breaking point following a series of territorial disputes over the Spratly Islands. A coalition of NATO naval forces has been dispatched to the region to protect international shipping lanes and deter further aggression.</p>

<h2>Forces</h2>
<p>The player commands a US Navy submarine operating as part of a larger task force. Enemy forces include a mix of diesel-electric and nuclear submarines, surface combatants, and maritime patrol aircraft.</p>

<h2>Objectives</h2>
<p>The primary objectives of the campaign include:</p>
<ul>
<li>Maintaining freedom of navigation through contested waters</li>
<li>Protecting allied shipping and naval vessels</li>
<li>Gathering intelligence on enemy force dispositions</li>
<li>Neutralising hostile submarine threats</li>
</ul>

<h2>Key Challenges</h2>
<div class=""article-image-text"">
<img src=""https://placehold.co/400x250/0a2a4a/6cb4d9?text=Sonar+Operations"" alt=""Sonar operations in shallow waters"" class=""article-image article-image-right"" />
<p>The shallow, warm waters of the South China Sea present unique challenges for submarine warfare. Thermal layers are unpredictable, and the busy shipping lanes create a noisy acoustic environment that complicates sonar operations.</p>
</div>",
                    CategoryId = "cat-1-1",
                    CategoryName = "South China Sea 2000 NATO",
                    Author = "admin",
                    DateCreated = new DateTime(2024, 5, 10),
                    DateModified = new DateTime(2024, 7, 5),
                    Summary = "Overview of the South China Sea 2000 NATO campaign scenario in Cold Waters.",
                    ThumbnailUrl = "https://placehold.co/400x200/16213e/87ceeb?text=South+China+Sea"
                },
                new EntryModel
                {
                    Id = "entry-3",
                    Title = "Will Mackenzie — Stalker Run Log",
                    Content = @"<h2>Day 1 — Mystery Lake</h2>
<img src=""https://placehold.co/800x300/2d4a3e/e0e0e0?text=Mystery+Lake+%E2%80%94+Day+1"" alt=""Mystery Lake on day one"" class=""article-image"" />
<p>Woke up in the wreckage. Temperature dropping fast. Found a hunting lodge near the lake — barely made it before the blizzard hit. Inventory: one granola bar, a flare, and a wool toque. Not exactly well-equipped for the apocalypse.</p>

<h2>Day 3 — The Dam</h2>
<p>Made my way to the Carter Hydro Dam. The interior is dark and labyrinthine, but at least it is sheltered from the wind. Found some canned food and a pry bar. Heard wolves outside — they are getting bolder.</p>

<h2>Day 7 — Coastal Highway</h2>
<div class=""article-image-text"">
<img src=""https://placehold.co/400x250/3a5a4e/c0d8c0?text=Coastal+Highway"" alt=""Coastal Highway"" class=""article-image article-image-right"" />
<p>Pushed east along the highway. The ice is treacherous; fell through twice. Managed to start a fire in the Quonset Garage before hypothermia set in. Found a rifle with three rounds. Every bullet counts on Stalker.</p>
</div>

<h2>Day 14 — Desolation Point</h2>
<p>Reached the old whaling station. Forged some arrowheads at the furnace — the bow is now my primary weapon. Food is scarce. Living on cattails and the occasional rabbit. The long dark is setting in.</p>",
                    CategoryId = "cat-2-1",
                    CategoryName = "Will Mackenzie (Stalker)",
                    Author = "admin",
                    DateCreated = new DateTime(2024, 7, 22),
                    DateModified = new DateTime(2024, 9, 1),
                    Summary = "Survival log following Will Mackenzie through a Stalker difficulty playthrough of The Long Dark.",
                    ThumbnailUrl = "https://placehold.co/400x200/2d4a3e/e0e0e0?text=Will+Mackenzie"
                },
                new EntryModel
                {
                    Id = "entry-4",
                    Title = "The Long Dark — Game Overview",
                    Content = @"<h2>About</h2>
<img src=""https://placehold.co/800x300/1c3d5a/a9d1e8?text=The+Long+Dark"" alt=""The Long Dark"" class=""article-image"" />
<p><strong>The Long Dark</strong> is a first-person survival game developed and published by Hinterland Studio. Set in the aftermath of a geomagnetic disaster, the player must survive in the frozen Canadian wilderness with no supernatural enemies — only the relentless cold, hunger, thirst, and wildlife.</p>

<h2>Game Modes</h2>
<p>The game offers several modes of play:</p>
<ul>
<li><strong>Wintermute</strong> — The story mode, following bush pilot Will Mackenzie and Dr. Astrid Greenwood after their plane crashes in the northern wilderness.</li>
<li><strong>Survival</strong> — An open-ended sandbox mode where the goal is simply to stay alive as long as possible.</li>
<li><strong>Challenges</strong> — Focused scenarios with specific objectives and conditions.</li>
</ul>

<h2>Difficulty Levels</h2>
<p>Survival mode offers four standard difficulty levels — Pilgrim, Voyageur, Stalker, and Interloper — plus a fully customisable mode. Stalker and Interloper are notably punishing, with aggressive wildlife, scarce resources, and extreme weather.</p>

<h2>Setting</h2>
<div class=""article-image-text"">
<img src=""https://placehold.co/400x250/2a4a6a/b0c8e0?text=Great+Bear+Island"" alt=""Great Bear Island"" class=""article-image article-image-right"" />
<p>The game takes place on Great Bear Island, a fictional location in the Canadian North. The interconnected regions range from dense forests and frozen lakes to abandoned mining towns and coastal highways.</p>
</div>",
                    CategoryId = "cat-2",
                    CategoryName = "The Long Dark",
                    Author = "admin",
                    DateCreated = new DateTime(2024, 4, 1),
                    DateModified = new DateTime(2024, 6, 15),
                    Summary = "Overview of The Long Dark, a first-person survival game set in the frozen Canadian wilderness.",
                    ThumbnailUrl = "https://placehold.co/400x200/1c3d5a/a9d1e8?text=The+Long+Dark"
                }
            };
        }

        private APIEntriesModel GetDummyEntries(string categoryId)
        {
            var allEntries = GetAllDummyEntries();
            var filtered = allEntries.Where(e => e.CategoryId == categoryId).ToList();

            // Also include entries from subcategories
            var categories = GetDummyCategories();
            var matchingSubIds = GetAllSubCategoryIds(categories, categoryId);
            filtered.AddRange(allEntries.Where(e => matchingSubIds.Contains(e.CategoryId) && !filtered.Any(f => f.Id == e.Id)));

            filtered = filtered.OrderBy(e => e.DateCreated).ToList();

            return new APIEntriesModel
            {
                Entries = filtered,
                MultiplePages = false,
                PageCount = 1,
                APICalled = true
            };
        }

        private List<string> GetAllSubCategoryIds(List<CategoryModel> categories, string parentId)
        {
            var ids = new List<string>();

            foreach (var cat in categories)
            {
                if (cat.Id == parentId)
                {
                    foreach (var sub in cat.SubCategories)
                    {
                        ids.Add(sub.Id);
                        ids.AddRange(GetAllSubCategoryIds(sub.SubCategories, sub.Id));
                    }
                }
                else
                {
                    ids.AddRange(GetAllSubCategoryIds(cat.SubCategories, parentId));
                }
            }

            return ids;
        }

        private EntryModel? GetDummyEntry(string entryId)
        {
            return GetAllDummyEntries().FirstOrDefault(e => e.Id == entryId);
        }

        private List<EntryModel> GetDummyRecentEntries()
        {
            return GetAllDummyEntries().OrderByDescending(e => e.DateModified).ToList();
        }

        #endregion
    }
}

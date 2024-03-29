using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AzureChatAPI.Controllers
{
    [ApiController]
    public class Chat2Controller : ControllerBase
    {
        private readonly IConfiguration Configuration;

        private readonly ILogger<Chat2Controller> _logger;

        public Chat2Controller(ILogger<Chat2Controller> logger, IConfiguration configuration)
        {
            Configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Route("chat2")]
        [Route("chat2/chat2")]
        [Route("azurechatapi/chat2")]
        public async IAsyncEnumerable<AnswerResults> PostChat2Async([FromBody] AIRequest request)
        {
            if (request != null && request.Messages != null)
            {
                AIConfig aiConfig = new AIConfig();
                aiConfig.Url = getString(Configuration["AI:Url"]);
                aiConfig.Key = getString(Configuration["AI:Key"]);
                aiConfig.Engine = getItem(getString(Configuration["AI:Engines"]), request.EngineIndex);

                AISearchConfig aiSearchConfig = new AISearchConfig();
                aiSearchConfig.Url = getString(Configuration["Search:Url"]);
                aiSearchConfig.Key = getString(Configuration["Search:Key"]);
                aiSearchConfig.Index = getItem(getString(Configuration["Search:Indexes"]), request.SearchIndex);

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                AnswerResults answerResults = new AnswerResults();
                var results = await AI.DoAsync2(aiConfig, aiSearchConfig, request);
                stopWatch.Stop();
                results.EllapsedMilliSeconts = stopWatch.ElapsedMilliseconds;
                _logger.LogInformation($"Answer produced in {results.EllapsedMilliSeconts} msecond");
                yield return results;
            }
            yield return new AnswerResults();
        }

        [HttpHead]
        [HttpGet]
        [Route("azurechatapi")]
        [Route("index")]
        [Route("chat2/index")]
        [Route("azurechatapi/index")]
        public void Get()
        {
            //testing method
        }

        protected string getString(string value)
        {
            if (value == null) return "";
            return value;
        }

        protected string getItem(string value, int index)
        {
            if (value == null) return "";
            var array = value.Split(',');
            return array[index];
        }
    }
}

﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Expressions.Task3.E3SQueryProvider.Models.Request
{
    [JsonObject]
    public class FtsQueryRequest
    {
        [JsonProperty("statements")]
        public List<Statement> Statements { get; set; }

        [JsonProperty("filters")]
        public List<Filter> Filters { get; set; }

        [JsonProperty("sorting")]
        public SortingCollection Sorting { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
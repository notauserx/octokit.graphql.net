﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Octokit.GraphQL.Core.Builders;
using Octokit.GraphQL.Core.UnitTests.Models;
using Xunit;

namespace Octokit.GraphQL.Core.UnitTests
{
    public class PagingTests
    {
        static readonly Subquery subqueryPlaceholder;

        public class Repository_Issues_AllPages
        {
            ICompiledQuery<IEnumerable<int>> TestQuery { get; } = new Query()
                .Repository("foo", "bar")
                .Issues()
                .AllPages()
                .Select(issue => issue.Number)
                .Compile();

            [Fact]
            public void Creates_MasterQuery()
            {
                var expected = @"query {
  repository(owner: ""foo"", name: ""bar"") {
    issues(first: 100) {
      pageInfo {
        hasNextPage
        endCursor
      }
      nodes {
        number
      }
    }
  }
}";

                var master = TestQuery.GetMasterQuery();

                Assert.Equal(expected, master.ToString());
            }

            [Fact]
            public void Creates_MasterQuery_Expression()
            {
                var expected = Expected(data =>
                    (IEnumerable<int>)Rewritten.List.ToSubqueryList(
                        Rewritten.List.Select(
                            data["data"]["repository"]["issues"]["nodes"],
                            issue => issue["number"].ToObject<int>()),
                        data.Annotation<IPagedQueryContext>(),
                        subqueryPlaceholder));

                var master = TestQuery.GetMasterQuery();

                Assert.Equal(expected, master.Expression.ToString());
            }

            [Fact]
            public void Creates_Subquery()
            {
                var expected = @"query($__id: ID!, $__after: String) {
  node(id: $__id) {
    ... on Repository {
      __typename
      issues(first: 100, after: $__after) {
        pageInfo {
          hasNextPage
          endCursor
        }
        nodes {
          number
        }
      }
    }
  }
}";

                var subqueries = TestQuery.GetSubqueries();

                Assert.Single(subqueries);
                Assert.Equal(expected, subqueries[0].Query.ToString());
            }

            [Fact]
            public void Creates_Subquery_PageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["node"]["issues"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Single(subqueries);
                Assert.Equal(expected, subqueries[0].PageInfo.ToString());
            }

            [Fact]
            public void Creates_Subquery_ParentPageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["repository"]["issues"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Single(subqueries);
                Assert.Equal(expected, subqueries[0].ParentPageInfo.ToString());
            }

            [Fact]
            public async Task Reads_All_Pages()
            {
                int page = 0;

                string Execute(string query)
                {
                    switch (page++)
                    {
                        case 0:
                            return @"{
  data: {
    ""repository"": {
      ""issues"": {
        ""pageInfo"": {
          ""hasNextPage"": true,
          ""endCursor"": ""end0""
        },
        ""nodes"": [
          { ""number"": 0 },
          { ""number"": 1 },
          { ""number"": 2 },
        ]
      }
    }
  }
}";
                        case 1:
                            return @"{
  data: {
    ""node"": {
      ""__typename"": ""Repository"",
      ""issues"": {
        ""pageInfo"": {
          ""hasNextPage"": false,
          ""endCursor"": ""end1""
        },
        ""nodes"": [
          { ""number"": 3 },
          { ""number"": 4 },
        ]
      }
    }
  }
}";
                        default:
                            throw new NotSupportedException("Should not get here");
                    }
                }

                var connection = new MockConnection(Execute);
                var result = (await connection.Run(TestQuery)).ToList();

                Assert.Equal(Enumerable.Range(0, 5).ToList(), result);
            }
        }

        public class Repository_Name_Issues_AllPages
        {
            ICompiledQuery<RepositoryModel> TestQuery { get; } = new Query()
                .Repository("foo", "bar")
                .Select(repository => new RepositoryModel
                {
                    Name = repository.Name,
                    Issues = repository.Issues(null, null, null, null, null).AllPages().Select(issue => new IssueModel
                    {
                        Number = issue.Number,
                    }).ToList()
                }).Compile();

            [Fact]
            public void Creates_MasterQuery()
            {
                var expected = @"query {
  repository(owner: ""foo"", name: ""bar"") {
    name
    issues(first: 100) {
      pageInfo {
        hasNextPage
        endCursor
      }
      nodes {
        number
      }
    }
  }
}";

                var master = TestQuery.GetMasterQuery();

                Assert.Equal(expected, master.ToString());
            }

            [Fact]
            public void Creates_MasterQuery_Expression()
            {
                var expected = Expected(data =>
                    Rewritten.Value.Select(
                        data["data"]["repository"],
                        repository => new RepositoryModel
                        {
                            Name = repository["name"].ToObject<string>(),
                            Issues = Rewritten.List.ToSubqueryList(
                                Rewritten.List.Select(
                                    repository["issues"]["nodes"],
                                    issue => new IssueModel
                                    {
                                        Number = issue["number"].ToObject<int>(),
                                    }),
                                data.Annotation<IPagedQueryContext>(),
                                subqueryPlaceholder),
                        }));

                var master = TestQuery.GetMasterQuery();

                Assert.Equal(expected, master.Expression.ToString());
            }

            [Fact]
            public void Creates_Subquery()
            {
                var expected = @"query($__id: ID!, $__after: String) {
  node(id: $__id) {
    ... on Repository {
      __typename
      issues(first: 100, after: $__after) {
        pageInfo {
          hasNextPage
          endCursor
        }
        nodes {
          number
        }
      }
    }
  }
}";

                var subqueries = TestQuery.GetSubqueries();

                Assert.Single(subqueries);
                Assert.Equal(expected, subqueries[0].Query.ToString());
            }

            [Fact]
            public void Creates_Subquery_PageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["node"]["issues"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Single(subqueries);
                Assert.Equal(expected, subqueries[0].PageInfo.ToString());
            }

            [Fact]
            public void Creates_Subquery_ParentPageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["repository"]["issues"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Single(subqueries);
                Assert.Equal(expected, subqueries[0].ParentPageInfo.ToString());
            }

            [Fact]
            public async Task Reads_All_Pages()
            {
                int page = 0;

                string Execute(string query)
                {
                    switch (page++)
                    {
                        case 0:
                            return @"{
  data: {
    ""repository"": {
      ""name"": ""foo"",
      ""issues"": {
        ""pageInfo"": {
          ""hasNextPage"": true,
          ""endCursor"": ""end0""
        },
        ""nodes"": [
          { ""number"": 0 },
          { ""number"": 1 },
          { ""number"": 2 },
        ]
      }
    }
  }
}";
                        case 1:
                            return @"{
  data: {
    ""node"": {
      ""__typename"": ""Repository"",
      ""issues"": {
        ""pageInfo"": {
          ""hasNextPage"": false,
          ""endCursor"": ""end1""
        },
        ""nodes"": [
          { ""number"": 3 },
          { ""number"": 4 },
        ]
      }
    }
  }
}";
                        default:
                            throw new NotSupportedException("Should not get here");
                    }
                }

                var connection = new MockConnection(Execute);
                var result = await connection.Run(TestQuery);

                Assert.Equal(
                    Enumerable.Range(0, 5).ToList(),
                    result.Issues.Select(x => x.Number).ToList());
            }
        }

        public class Repository_Issues_Comments_AllPages
        {
            ICompiledQuery<IEnumerable<IssueModel>> TestQuery { get; } = new Query()
                .Repository("foo", "bar")
                .Issues()
                .AllPages()
                .Select(issue => new IssueModel
                {
                    Number = issue.Number,
                    Comments = issue.Comments(null, null, null, null).AllPages().Select(comment => new CommentModel
                    {
                        Body = comment.Body,
                    }).ToList(),
                })
                .Compile();

            [Fact]
            public void Creates_MasterQuery()
            {
                var expected = @"query {
  repository(owner: ""foo"", name: ""bar"") {
    issues(first: 100) {
      pageInfo {
        hasNextPage
        endCursor
      }
      nodes {
        number
        comments(first: 100) {
          pageInfo {
            hasNextPage
            endCursor
          }
          nodes {
            body
          }
        }
      }
    }
  }
}";

                var master = TestQuery.GetMasterQuery();

                Assert.Equal(expected, master.ToString());
            }

            [Fact]
            public void Creates_MasterQuery_Expression()
            {
                var subquery = new Subquery();
                var expected = Expected(data =>
                    (IEnumerable<IssueModel>)Rewritten.List.ToSubqueryList(
                        Rewritten.List.Select(
                            data["data"]["repository"]["issues"]["nodes"],
                            issue => new IssueModel
                            {
                                Number = issue["number"].ToObject<int>(),
                                Comments = Rewritten.List.ToSubqueryList(
                                    Rewritten.List.Select(
                                        issue["comments"]["nodes"],
                                        comment => new CommentModel
                                        {
                                            Body = comment["body"].ToObject<string>(),
                                        }),
                                    data.Annotation<IPagedQueryContext>(),
                                    subqueryPlaceholder)
                            }),
                        data.Annotation<IPagedQueryContext>(),
                        subqueryPlaceholder));

                var master = TestQuery.GetMasterQuery();

                Assert.Equal(expected, master.Expression.ToString());
            }

            [Fact]
            public void Creates_Subquery_1()
            {
                var expected = @"query($__id: ID!, $__after: String) {
  node(id: $__id) {
    ... on Repository {
      __typename
      issues(first: 100, after: $__after) {
        pageInfo {
          hasNextPage
          endCursor
        }
        nodes {
          number
          comments(first: 100) {
            pageInfo {
              hasNextPage
              endCursor
            }
            nodes {
              body
            }
          }
        }
      }
    }
  }
}";

                var subqueries = TestQuery.GetSubqueries();

                Assert.Equal(2, subqueries.Count);
                Assert.IsType<PagedQuery<IEnumerable<IssueModel>>>(subqueries[0].Query);

                var query = (PagedQuery<IEnumerable<IssueModel>>)subqueries[0].Query;
                Assert.Equal(expected, query.GetMasterQuery().ToString());
            }

            [Fact]
            public void Creates_Subquery_1_PageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["node"]["issues"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Equal(2, subqueries.Count);
                Assert.Equal(expected, subqueries[0].PageInfo.ToString());
            }

            [Fact]
            public void Creates_Subquery_1_ParentPageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["repository"]["issues"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Equal(2, subqueries.Count);
                Assert.Equal(expected, subqueries[0].ParentPageInfo.ToString());
            }

            [Fact]
            public void Creates_Subquery_2()
            {
                var expected = @"query($__id: ID!, $__after: String) {
  node(id: $__id) {
    ... on Issue {
      __typename
      comments(first: 100, after: $__after) {
        pageInfo {
          hasNextPage
          endCursor
        }
        nodes {
          body
        }
      }
    }
  }
}";

                var subqueries = TestQuery.GetSubqueries();

                Assert.Equal(2, subqueries.Count);
                Assert.Equal(expected, subqueries[1].Query.ToString());
            }

            [Fact]
            public void Creates_Subquery_2_PageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["node"]["comments"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Equal(2, subqueries.Count);
                Assert.Equal(expected, subqueries[1].PageInfo.ToString());
            }

            [Fact]
            public void Creates_Subquery_2_ParentPageInfo_Selector()
            {
                var expected = Expected(data => data["data"]["repository"]["issues"]["nodes"]["comments"]["pageInfo"]);
                var subqueries = TestQuery.GetSubqueries();

                Assert.Equal(2, subqueries.Count);
                Assert.Equal(expected.ToString(), subqueries[1].ParentPageInfo.ToString());
            }
        }

        private static string Expected<T>(Expression<Func<JObject, T>> expression)
        {
            var str = expression.ToString();

            // There's no way to get "value(Octokit.GraphQL.Core.Subquery)" string in an expression
            // string from a lambda, so put in `subqueryPlaceholder` and replace it.
            str = str.Replace("PagingTests.subqueryPlaceholder", "value(Octokit.GraphQL.Core.Subquery)");

            return str;
        }

        class RepositoryModel
        {
            public string Name { get; set; }
            public IList<IssueModel> Issues { get; set; }
        }

        class IssueModel
        {
            public int Number { get; set; }
            public IList<CommentModel> Comments { get; set; }
        }

        class CommentModel
        {
            public string Body { get; set; }
        }
    }
}

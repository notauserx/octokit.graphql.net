namespace Octokit.GraphQL.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Octokit.GraphQL.Core;
    using Octokit.GraphQL.Core.Builders;

    /// <summary>
    /// Autogenerated return type of DeleteIssueComment
    /// </summary>
    public class DeleteIssueCommentPayload : QueryableValue<DeleteIssueCommentPayload>
    {
        internal DeleteIssueCommentPayload(Expression expression) : base(expression)
        {
        }

        /// <summary>
        /// A unique identifier for the client performing the mutation.
        /// </summary>
        public string ClientMutationId { get; }

        internal static DeleteIssueCommentPayload Create(Expression expression)
        {
            return new DeleteIssueCommentPayload(expression);
        }
    }
}
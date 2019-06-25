namespace Octokit.GraphQL.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Autogenerated input type of RemoveAssigneesFromAssignable
    /// </summary>
    public class RemoveAssigneesFromAssignableInput
    {
        /// <summary>
        /// The id of the assignable object to remove assignees from.
        /// </summary>
        public ID AssignableId { get; set; }

        /// <summary>
        /// The id of users to remove as assignees.
        /// </summary>
        public IEnumerable<ID> AssigneeIds { get; set; }

        /// <summary>
        /// A unique identifier for the client performing the mutation.
        /// </summary>
        public string ClientMutationId { get; set; }
    }
}
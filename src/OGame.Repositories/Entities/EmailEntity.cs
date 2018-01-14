using System;

namespace OGame.Repositories.Entities
{
    public class EmailEntity
    {
        public Guid Id { get; set; }

        public string Recipient { get; set; }

        public string Sender { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }

        public bool IsDelivered { get; set; }


        public DateTime Created { get; set; }

        public DateTime LastSent { get; set; }

        public int FailedTimes { get; set; }
    }
}

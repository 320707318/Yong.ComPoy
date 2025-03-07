using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels
{
    public class SuperTopic
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long CreateTime { get; set; }= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public long CreatorId { get; set; }

        public int Number { get; set; }
        public string AvatarUrl { get; set; }
        public long TypeId { get; set; }
        public int Status { get; set; }=0;
        [Navigate(nameof(TypeId))]
        public SuperTopicType Type { get; set; }
    }
    public class SuperTopicType
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; } = 1;
        public long CreateTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

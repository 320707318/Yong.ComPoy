using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Cap.Mq
{
    public class SuperTopicMqDto
    {
    }
    public class SuperTopicAddMqDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long CreatorId { get; set; } = 0;

        public string AvatarUrl { get; set; }
        public long TypeId { get; set; }
    }
    public class SuperTopicTypeAddMqDto
    {
        public string Name { get; set; }
        public int Sort { get; set; }
    }
}

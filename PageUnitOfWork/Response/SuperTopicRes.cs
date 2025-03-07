using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels;
using FreeSql.DataAnnotations;

namespace PageUnitOfWork.Response
{
    public class SuperTopicRes
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Number { get; set; }
        public string AvatarUrl { get; set; }
        public long CreatorId { get; set; }
        public TypeRes type { get; set; }
        public long TypeId { get; set; }
        public bool IsJoin { get; set; }
    }
    public class SuperTopicSimpleRes
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }
    public class SuperTopicDetailRes: SuperTopicRes
    {
        public long CreatorId { get; set; }
        public UserRes User { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Nest;
using Yong.Page.Api.DataContracts;
using static Yong.Page.Api.DataContracts.PageApi;

namespace PageUnitOfWork.gRpcServices
{
    public class PageService:PageApiBase
    {
        //private readonly ElasticClient _client;

        //public PageService(ElasticClient client)
        //{
        //    this._client = client;
        //}
        override public async Task<PageRes> GetPageById(GetPageReq request, ServerCallContext context)
        {
           // var pageIndex = await _client.SearchAsync<PageRes>(s =>
           //    s.Index("page")
           //    .From(0)
           //    .Size(1)
           //    .Query(q => q.Match(m => m.Field("id").Query(request.Id.ToString())))
           //);
           // if(!pageIndex.IsValid)
           // {
           //     await Console.Out.WriteLineAsync();
           // }
           // var page = pageIndex.Documents.First();
           // var res= new PageRes
           // {
           //     Id = page.Id,
           //     Title = page.Title,
           //     Content = page.Content,
           //     CreateTime = page.CreateTime,

           // };
           // res.Resources.AddRange(page.Resources);
            return new PageRes { };
        }
    }
}

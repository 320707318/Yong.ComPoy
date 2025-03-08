using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendProvide.QQEmali.Models
{
    public static class EmaliTemplet
    {
        public  enum TempletType
        {
            Login,
            Forget
        }
        public static string GetTemplet(int Code,int Type=0,string name="")
        {
            string TypeName = "登录";
            switch (Type)
            {
                case 1:
                    TypeName = "忘记密码找回";
                    break;
            }


            string templet = @"<!DOCTYPE html>
            <html lang=""en"">
            
            <head>
              <meta charset=""UTF-8"">
              <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>YONG.COMMUNITY</title>
            
              <style>
                body,html,div,ul,li,button,p,img,h1,h2,h3,h4,h5,h6 {
                  margin: 0;
                  padding: 0;
                }
            
                body,html {
                  background: #fff;
                  line-height: 1.8;
                }
            
                h1,h2,h3,h4,h5,h6 {
                  line-height: 1.8;
                }
            
                .email_warp {
                  height: 100vh;
                  min-height: 500px;
                  font-size: 14px;
                  color: #212121;
                  display: flex;
                  /* align-items: center; */
                  justify-content: center;
                }
            
                .logo {
                  margin: 3em auto;
                  width: 200px;
                  height: 60px;
                }
            
                h1.email-title {
                  font-size: 26px;
                  font-weight: 500;
                  margin-bottom: 15px;
                  color: #252525;
                }
            
                a.links_btn {
                  border: 0;
                  background: #4C84FF;
                  color: #fff;
                  width: 100%;
                  height: 50px;
                  line-height: 50px;
                  font-size: 16px;
                  margin: 40px auto;
                  box-shadow: 0px 2px 4px 0px rgba(0, 0, 0, 0.15);
                  border-radius: 4px;
                  outline: none;
                  cursor: pointer;
                  transition: all 0.3s;
                  text-align: center;
                  display: block;
                  text-decoration: none;
                }
            
                .warm_tips {
                  color: #757575;
                  background: #f7f7f7;
                  padding: 20px;
                }
            
                .warm_tips .desc {
                  margin-bottom: 20px;
                }
            
                .qr_warp {
                  max-width: 140px;
                  margin: 20px auto;
                }
            
                .qr_warp img {
                  max-width: 100%;
                  max-height: 100%;
                }
            
                .email-footer {
                  margin-top: 2em;
                }
            
                #reset-password-email {
                  max-width: 500px;
                }
                #reset-password-email .accout_email {
                  color: #4C84FF;
                  display: block;
                  margin-bottom: 20px;
                }
              </style>
            </head>
            
            <body>
              <section class=""email_warp"">
                <div id=""reset-password-email"">
                  <div class=""logo"">
                    <img src=""https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/V1Image/icon.png"" alt=""logo"" style=""width: 70px;height: 70;"">
                  </div>
            
                  <h1 class=""email-title"">
                    尊敬的<span>" + name + @"</span>您好：
                  </h1>

                   <h1 class=""email-title"">
                    您的" + TypeName + @"验证码为:<span>"+ Code + @"</span>
                  </h1>
                  <p>请注意，如果这不是您本人的操作，请忽略并关闭此邮件。</p>
            
            
                  <div class=""warm_tips"">
                    <div class=""desc"">
                      为安全起见，以上为一次性链接，且仅在5分钟内有效，请您尽快完成操作。
                    </div>
            

                    <p>本邮件由系统自动发送，请勿回复。</p>
                  </div>
            
                  <div class=""email-footer"">
                    <p>您的智能项目助理</p>
                    <p>Yong.COMPOY</p>
                  </div>
                </div>
              </section>
            </body>
            
            </html>
            ";
            return templet;
        }
        public static string GetAuditTemplet(string ShopName, string Message = "",string Reson = "")
        {
            string templet = @"<!DOCTYPE html>
            <html lang=""en"">
            
            <head>
              <meta charset=""UTF-8"">
              <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>YONG.COMMUNITY</title>
            
              <style>
                body,html,div,ul,li,button,p,img,h1,h2,h3,h4,h5,h6 {
                  margin: 0;
                  padding: 0;
                }
            
                body,html {
                  background: #fff;
                  line-height: 1.8;
                }
            
                h1,h2,h3,h4,h5,h6 {
                  line-height: 1.8;
                }
            
                .email_warp {
                  height: 100vh;
                  min-height: 500px;
                  font-size: 14px;
                  color: #212121;
                  display: flex;
                  /* align-items: center; */
                  justify-content: center;
                }
            
                .logo {
                  margin: 3em auto;
                  width: 200px;
                  height: 60px;
                }
            
                h1.email-title {
                  font-size: 26px;
                  font-weight: 500;
                  margin-bottom: 15px;
                  color: #252525;
                }
            
                a.links_btn {
                  border: 0;
                  background: #4C84FF;
                  color: #fff;
                  width: 100%;
                  height: 50px;
                  line-height: 50px;
                  font-size: 16px;
                  margin: 40px auto;
                  box-shadow: 0px 2px 4px 0px rgba(0, 0, 0, 0.15);
                  border-radius: 4px;
                  outline: none;
                  cursor: pointer;
                  transition: all 0.3s;
                  text-align: center;
                  display: block;
                  text-decoration: none;
                }
            
                .warm_tips {
                  color: #757575;
                  background: #f7f7f7;
                  padding: 20px;
                }
            
                .warm_tips .desc {
                  margin-bottom: 20px;
                }
            
                .qr_warp {
                  max-width: 140px;
                  margin: 20px auto;
                }
            
                .qr_warp img {
                  max-width: 100%;
                  max-height: 100%;
                }
            
                .email-footer {
                  margin-top: 2em;
                }
            
                #reset-password-email {
                  max-width: 500px;
                }
                #reset-password-email .accout_email {
                  color: #4C84FF;
                  display: block;
                  margin-bottom: 20px;
                }
              </style>
            </head>
            
            <body>
              <section class=""email_warp"">
                <div id=""reset-password-email"">
                  <div class=""logo"">
                    <img src=""https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/V1Image/icon.png"" alt=""logo"" style=""width: 70px;height: 70;"">
                  </div>
            
                  <h1 class=""email-title"">
                    尊敬的商家<span>" + ShopName + @"</span>您好：
                  </h1>

                   <h1 class=""email-title"">
                    您的注册商家申请" + @"<span>" + Message + @"</span>
                  </h1>
                  <p>请注意，如果这不是您本人的操作，请忽略并关闭此邮件。</p>
            
            
                  <div class=""warm_tips"">
                    <div class=""desc"">"
                      +Reson+ @"
                    </div>
            

                    <p>本邮件由系统自动发送，请勿回复。</p>
                  </div>
            
                  <div class=""email-footer"">
                    <p>您的智能项目助理</p>
                    <p>Yong.COMPOY</p>
                  </div>
                </div>
              </section>
            </body>
            
            </html>
            ";
            return templet;
        }
    }
}

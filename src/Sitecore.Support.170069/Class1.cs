using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.ExperienceEditor.Speak.Ribbon.Requests.Breadcrumb;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Globalization;
using Sitecore.ItemWebApi.Pipelines.GetProperties;
using Sitecore.Links;


namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.Breadcrumb
{
    public class GetBreadcrumbStructure : Sitecore.ExperienceEditor.Speak.Ribbon.Requests.Breadcrumb.GetBreadcrumbStructure
    {
        public override PipelineProcessorResponseValue ProcessRequest()
        {
            if (base.RequestContext.Item == null)
            {
                return new PipelineProcessorResponseValue
                {
                    AbortMessage = Translate.Text("The target item could not be found.")
                };
            }
            List<Item> list = this.CollectItems(this.ResolveItem());
            list.Reverse();
            IEnumerable<BreadcrumbItem> value = from child in list
                select new SupportBreadcrumbItem(child, base.RequestContext.DeviceItem);
            return new PipelineProcessorResponseValue
            {
                Value = value
            };
        }
    }

    public class GetChildItems : PipelineProcessorRequest<ItemContext>
    {
        public override PipelineProcessorResponseValue ProcessRequest()
        {
            if (base.RequestContext.Item == null)
            {
                return new PipelineProcessorResponseValue
                {
                    AbortMessage = Translate.Text("The target item could not be found.")
                };
            }
            List<BreadcrumbItem> list = new List<BreadcrumbItem>();
            foreach (Item item in base.RequestContext.Item.Children)
            {
                list.Add(new SupportBreadcrumbItem(item, base.RequestContext.DeviceItem));
            }
            return new PipelineProcessorResponseValue
            {
                Value = list
            };
        }
    }


    public class SupportBreadcrumbItem : Sitecore.ExperienceEditor.Speak.Ribbon.Requests.Breadcrumb.BreadcrumbItem
    {
        public SupportBreadcrumbItem(Item item, DeviceItem deviceItem):base(item,deviceItem)
        {
            //fix for the issue 170069 is to use the GetUIDisplayName() instead of item.DisplayName
            this.DisplayName = item.GetUIDisplayName();
        }
    }
}

namespace Sitecore.Support.ItemWebApi.Pipelines.GetProperties
{
    public class GetProperties : Sitecore.ItemWebApi.Pipelines.GetProperties.GetPropertiesProcessor
    {
        public override void Process(GetPropertiesArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            Item item = arguments.Item;
            SortedDictionary<string, object> properties = arguments.Properties;
            Assert.IsNotNull(item.Template, "Template is null.");
            properties.Add("Database", item.Database.Name);
            //fix for the issue 170069 is to use the GetUIDisplayName() instead of item.DisplayName
            properties.Add("DisplayName", item.GetUIDisplayName());
            properties.Add("HasChildren", item.HasChildren);
            properties.Add("ID", item.ID.ToString());
            properties.Add("Language", item.Language.ToString());
            properties.Add("LongID", item.Paths.LongID);
            properties.Add("Path", item.Paths.FullPath);
            properties.Add("Template", item.Template.FullName);
            properties.Add("Version", item.Version.Number);
        }
    }
}
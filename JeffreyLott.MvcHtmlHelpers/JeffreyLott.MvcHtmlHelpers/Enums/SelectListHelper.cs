using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Web.Mvc.Html;

namespace JeffreyLott.MvcHtmlHelpers.Enums
{
    public static class SelectListHelper
    {
        public static HtmlString DropDownListForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> enumExpression, string selectText = "")
        {
            var enumType = enumExpression.ReturnType;
            if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                enumType = Nullable.GetUnderlyingType(enumType);
            }
            if (!enumType.IsEnum)
                return null;

            var enumValue = enumExpression.Compile().Invoke(htmlHelper.ViewData.Model);
            var selectListItems = GenerateSelectListItems<TProperty>(enumValue);

            if (!String.IsNullOrEmpty(selectText))
            {
                selectListItems.Insert(0, new SelectListItem() { Text = selectText });
            }

            // Convert each ListItem to an <option> tag
            StringBuilder listItemBuilder = new StringBuilder();

            foreach (SelectListItem item in selectListItems)
            {
                listItemBuilder.AppendLine(ListItemToOption(item));
            }

            TagBuilder tagBuilder = new TagBuilder("select")
            {
                InnerHtml = listItemBuilder.ToString()
            };

            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(enumExpression));

            tagBuilder.MergeAttribute("name", fullName, true /* replaceExisting */);
            tagBuilder.GenerateId(fullName);

            // If there are any errors for a named field, we add the css attribute.
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullName, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            tagBuilder.MergeAttributes(htmlHelper.GetUnobtrusiveValidationAttributes(ExpressionHelper.GetExpressionText(enumExpression)));

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }

        private static List<SelectListItem> GenerateSelectListItems<TProperty>(TProperty currentValue)
        {
            var t = typeof(TProperty);
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                t = Nullable.GetUnderlyingType(t);
            }

            if (t.IsEnum)
            {
                var items = new List<SelectListItem>();

                foreach (var value in t.GetFields())
                {
                    var attribute = Attribute.GetCustomAttribute(value, typeof(DescriptionAttribute)) as DescriptionAttribute;

                    if (attribute != null)
                    {
                        int val = (int)value.GetValue(null);
                        items.Add(new SelectListItem() { Text = attribute.Description, Value = val.ToString(), Selected = value.Name == currentValue.ToString() });
                    }
                }

                return items;
            }

            return null;
        }

        private static string ListItemToOption(SelectListItem item)
        {
            TagBuilder builder = new TagBuilder("option")
            {
                InnerHtml = HttpUtility.HtmlEncode(item.Text)
            };
            if (item.Value != null)
            {
                builder.Attributes["value"] = item.Value;
            }
            if (item.Selected)
            {
                builder.Attributes["selected"] = "selected";
            }
            return builder.ToString(TagRenderMode.Normal);
        }
    }
}

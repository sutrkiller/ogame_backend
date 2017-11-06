using Microsoft.AspNetCore.Mvc.Filters;
using OGame.Api.Helpers;

namespace OGame.Api.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!context.ModelState.IsValid)
            {
                context.Result = ApiErrors.InvalidModel(context.ModelState);
            }
        }
    }
}

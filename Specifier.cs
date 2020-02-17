using System.Linq;
using System.Reflection;

namespace Documentation
{
    public class Specifier<T> : ISpecifier
    {
        public string GetApiDescription()
        {
            var r = typeof(T).GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute;

            return r?.Description;
        }

        public string[] GetApiMethodNames()
        {
            return typeof(T).GetMethods()
                .Where(x => x.GetCustomAttribute(typeof(ApiMethodAttribute)) != null)
                .Select(x => x.Name)
                .ToArray();
        }

        public string GetApiMethodDescription(string methodName)
        {
            var method = typeof(T).GetMethod(methodName);
            var r = (method?.GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute);

            return r?.Description;
        }

        public string[] GetApiMethodParamNames(string methodName)
        {
            var method = typeof(T).GetMethod(methodName);
            var r = method?.GetParameters().Select(x => x.Name).ToArray();

            return r;
        }

        public string GetApiMethodParamDescription(string methodName, string paramName)
        {
            var method = typeof(T).GetMethod(methodName);
            var param = method?.GetParameters().FirstOrDefault(x => x.Name == paramName);
            var r = param?.GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute;

            return r?.Description;
        }

        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName)
        {
            var method = typeof(T).GetMethod(methodName);
            var param = method?.GetParameters().FirstOrDefault(x => x.Name == paramName);

            var mm = param?.GetCustomAttribute(typeof(ApiIntValidationAttribute)) as ApiIntValidationAttribute;
            var req = param?.GetCustomAttribute(typeof(ApiRequiredAttribute)) as ApiRequiredAttribute;

            var cd = new CommonDescription
            {
                Name = paramName,
                Description = GetApiMethodParamDescription(methodName, paramName)
            };
            var r = new ApiParamDescription
            {
                MinValue = mm?.MinValue,
                MaxValue = mm?.MaxValue,
                ParamDescription = cd,
                Required = req?.Required ?? false
            };

            return r;
        }

        public ApiMethodDescription GetApiMethodFullDescription(string methodName)
        {
            var method = typeof(T).GetMethod(methodName);

            if (method.GetCustomAttribute(typeof(ApiMethodAttribute)) == null)
                return null;

            var paramss = method?.GetParameters()
                .Select(param => GetApiMethodParamFullDescription(methodName, param.Name))
                .ToArray();

            var cd = new CommonDescription
            {
                Name = methodName,
                Description = GetApiMethodDescription(methodName)
            };
            var r = new ApiMethodDescription
            {
                MethodDescription = cd,
                ParamDescriptions = paramss,
                ReturnDescription = GetReturnDescription(method)
            };
            return r;
        }

        private ApiParamDescription GetReturnDescription(MethodInfo method)
        {
            ApiParamDescription r = null;
            var req = method?.ReturnParameter.GetCustomAttribute(typeof(ApiRequiredAttribute)) as ApiRequiredAttribute;
            var mm = method?.ReturnParameter.GetCustomAttribute(typeof(ApiIntValidationAttribute)) as ApiIntValidationAttribute;

            if (req != null || mm != null)
                r = new ApiParamDescription
                {
                    MinValue = mm?.MinValue,
                    MaxValue = mm?.MaxValue,
                    Required = req?.Required ?? false
                };

            return r;
        }
    }
}
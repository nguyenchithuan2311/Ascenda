using System.Reflection;

namespace Test.Utils;


public class MergerObject
{
    public static void MergeObjects<T>(T objA, T objB) where T : class, new()
    {
        if (objA == null || objB == null)
            throw new ArgumentNullException("Objects cannot be null");

        var properties = typeof(T).GetProperties(BindingFlags.Public  | BindingFlags.Instance );

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            var valueB = property.GetValue(objB);
            var valueA = property.GetValue(objA);

            if (IsDefaultValue(valueB)||valueB==null) continue;

            // Nếu là class (trừ string), thực hiện đệ quy
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                if (IsDefaultValue(objA.GetType().GetProperty(property.Name).GetValue(objA)))
                {
                    var newValue = Activator.CreateInstance(property.PropertyType);
                    newValue= property.GetValue(objB);
                    property.SetValue(objA, newValue);
                }

                // Gọi đệ quy, cần truyền kiểu cụ thể của thuộc tính con
                MergeObjects(valueA, valueB, property.PropertyType);
            }
            else if (IsDefaultValue(objA)||objA==null)
            {
                // Nếu thuộc tính không phải class hoặc string, gán trực tiếp
                property.SetValue(objA, valueB);
            }
        }
    }

    // Overload cho các kiểu con
    private static void MergeObjects(object objA, object objB, Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance );

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            try
            {
                var valueB = property.GetValue(objB);
                var valueA = property.GetValue(objA);
                if (valueB == null || IsDefaultValue(valueB)) continue;
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    if (IsDefaultValue(objA.GetType().GetProperty(property.Name).GetValue(objA)))
                    {
                        property.SetValue(objA, valueB);
                    }

                    MergeObjects(valueA, valueB, property.PropertyType);
                    
                }
                else if (IsDefaultValue(objA)||objA==null)
                {
                    property.SetValue(objA, valueB);
                }
            }
            catch
            {
                continue;
            }

            
        }
    }
    
    private static bool IsDefaultValue(object value)
{
    if (value == null)
        return true;

    var type = value.GetType();

    // Kiểm tra đối với kiểu giá trị (value types)
    if (type.IsValueType)
    {
        // Kiểm tra kiểu số (int, float, double, v.v.)
        if (type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return Convert.ToDecimal(value) == 0; // Giá trị mặc định của số là 0
        }

        // Kiểm tra kiểu bool
        if (type == typeof(bool))
        {
            return (bool)value == false; // Giá trị mặc định của bool là false
        }

        // Kiểm tra kiểu Enum (giá trị mặc định là giá trị đầu tiên của Enum)
        if (type.IsEnum)
        {
            return value.Equals(Activator.CreateInstance(type)); // Kiểm tra xem có phải giá trị mặc định của Enum không
        }

        return value.Equals(Activator.CreateInstance(type));  // Kiểm tra giá trị mặc định cho các kiểu giá trị khác
    }

    // Kiểm tra đối với kiểu tham chiếu (reference types)
    if (type == typeof(string))
    {
        return string.IsNullOrEmpty((string)value);  // Kiểm tra chuỗi rỗng hoặc null
    }

    // Kiểm tra đối với danh sách (List hoặc ICollection)
    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
    {
        var list = value as System.Collections.IEnumerable;
        return list == null || !list.GetEnumerator().MoveNext();  // Kiểm tra nếu danh sách rỗng
    }

    // Kiểm tra đối với các kiểu class đã khởi tạo, kiểm tra tất cả các thuộc tính
    if (type.IsClass)
    {
        // Kiểm tra nếu tất cả các thuộc tính của đối tượng là giá trị mặc định
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            var valueOfProperty = property.GetValue(value);
            if (!IsDefaultValue(valueOfProperty))
                return false; // Nếu có bất kỳ thuộc tính nào không phải giá trị mặc định, trả về false
        }
        return true; // Nếu tất cả các thuộc tính đều có giá trị mặc định, trả về true
    }

    return false;  // Các kiểu tham chiếu khác (class) mặc định là null
}

}
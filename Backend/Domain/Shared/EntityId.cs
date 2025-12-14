namespace ProjArqsi.Domain.Shared
{
    public abstract class EntityId: IEquatable<EntityId>, IComparable<EntityId>
    {
        protected object ObjValue {get;}

        public string Value { 
            get { 
                    if (ObjValue.GetType() == typeof(string))
                        return (string) ObjValue;
                    return AsString();
                } 
        }

        public EntityId(object value)
        {
            if (value.GetType() == typeof(string))
                ObjValue = CreateFromString((string)value);
            else
                ObjValue = value;
        }

        public virtual object CreateFromString(string text){
            return new Guid(text);
        }
        public string AsString(){
            return ObjValue.ToString() ?? string.Empty;
        }


        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(EntityId? other)
        {
            if (other == null)
                return false;
            if (GetType() != other.GetType())
                return false;
            return Value == other.Value;
        }

        public int CompareTo(EntityId? other){
            if (other == null)
                return -1;
            return Value.CompareTo(other.Value);
        }

        public static bool operator ==(EntityId? obj1, EntityId? obj2)
        {
            if (Equals(obj1, null))
            {
                if (Equals(obj2, null))
                    return true;
                return false;
            }
            return obj1.Equals(obj2);
        }
        public static bool operator !=(EntityId? x, EntityId? y) 
        {
            return !(x == y);
        }
    }
   
}
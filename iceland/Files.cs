
internal static class Files
{
    public static string ProceduresMetaSQL()
    {
        return @$"
		
DECLARE @Result TABLE (ObjectId INT, [Name] VARCHAR(max), [Schema] VARCHAR(max),Parameters nvarchar(max), ReturnType nvarchar(max), Dependencies nvarchar(max)) ; 

INSERT INTO  @Result (ObjectId, [Name], [Schema])  
select 
p.object_id,
p.name as [Name],
s.name as [Schema] 
from sys.procedures p
inner join sys.schemas s on p.schema_id=s.schema_id 
where 
p.is_ms_shipped=0 
order by p.name  


-- Get Parameters
update @Result set Parameters = (
    (
    select   
        right(parameters.name, len(parameters.name) - 1) as Name,
        types.name as Type, 
        case when types.name='varchar' and parameters.max_length<0 then 2000 else parameters.max_length end as Length,
        parameters.is_output as IsOut,
        types.is_user_defined as IsUserDefinedType, 
        isnull( types.is_table_type,0 ) as IsUserDefinedTableType
    from sys.parameters parameters 
        left join sys.types types
        on types.system_type_id = parameters.system_type_id and types.user_type_id=parameters.user_type_id		
    where parameters.object_id= res.ObjectId
    for JSON Path 
    ) 
)
from @Result res

-- Get ReturnType
update @Result set ReturnType = (
    (
    SELECT 
    r.system_type_name as Type, 
    r.name as Name, 
    r.is_nullable as IsNullable, 
    r.max_length as Length ,
    (case when r.error_type_desc='CONFLICTING_RESULTS' then '1' else 0 end) as IsMultiResult  
    FROM  sys.dm_exec_describe_first_result_set_for_object(res.ObjectId, 0)  AS r 
    for JSON Path
    ) 
)
from @Result res

-- Dependencies
UPDATE @Result set Dependencies=(
    SELECT  referenced_entity_name as ChildProcedure 
FROM sys.sql_expression_dependencies AS sed  
INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id  
WHERE referencing_id = res.ObjectId
order by charindex(referenced_entity_name,OBJECT_DEFINITION(res.ObjectId))
for Json PATH
)
from @Result res

SELECT * from @Result 
--for Json path

		
		";
    }
}




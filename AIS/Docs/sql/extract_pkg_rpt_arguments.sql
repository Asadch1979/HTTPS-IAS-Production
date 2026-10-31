SELECT package_name,object_name,overload,argument_name,position,data_type,in_out
FROM user_arguments
WHERE package_name='PKG_RPT' AND object_name='R_IAS_PARA_POSITION'
ORDER BY overload,sequence;

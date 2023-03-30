local Test = {}
Test.num = 1
print("Lua Test++++++++++")

function Test:new()
    local o = {}
    o.num = 3
    setmetatable(o, {__index = self})
    return o
end

function Test:GetNum()
end

function Test:CollectGarbage()
    local a = {}
    setmetatable(a, {})
    local k = {}
    a[k] = 2
    k = nil
    
    collectgarbage()
    for i, v in pairs(a) do
        print("CollectGarbage+++++++ " .. v);
        print(i)
    end
end


function Test:ClosureTest()
    local func = function()
        local i = 0
        return function() i = i + 1 print("ClosureTest++++++ " .. i) end
    end
    local c1 = func()
    c1()
    c1()
end

return Test
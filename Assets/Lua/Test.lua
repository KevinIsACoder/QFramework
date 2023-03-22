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
    print("Test Lua+++++++ " .. self.num)
end

return Test
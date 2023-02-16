local BilliardsProxy = {}
local self = BilliardsProxy
local EventDef = BillardsEvent.BillardsEventType
function BilliardsProxy.RegisterEvent(eventManager)
    eventManager:Add(BilliardsNetDefine.Net2C_5001, function(_, data)
        self.Net2C_5001(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5003, function(_, data)
        self.Net2C_5003(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5101, function(_, data)
        self.Net2C_5101(data)
    end)
    
    eventManager:Add(BilliardsNetDefine.Net2C_5004, function(_, data)
        self.Net2C_5004(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5005, function(_, data)
        self.Net2C_5005(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5006, function(_, data)
        self.Net2C_5006(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5002, function(_, data)
        self.Net2C_5002(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5102, function(_, data)
        self.ChangeCueDirAndPosRes(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5103, function(_, data)
        self.Net2C_5103(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5007, function(_, data)
        self.Net2C_5007(data)
    end)

    eventManager:Add(BilliardsNetDefine.Net2C_5106, function(_, data)
        self.Net2C_5106(data)
    end)
end

function BilliardsProxy.Net2C_5007(data)
    if data then
        for _, user in pairs(data.user_can_hit_balls) do
            local player = BilliardsBattle.GetPlayerByPos(user.pos)
            if player then player:SetCanHitBalls(user.can_hit_balls) end
        end
    end

    -- 同步球的位置，这里最好加上pos
    BilliardsBattle.SyncBalls(data)

    EventManager.Dispatch(EventDef.E_UpdateBall)
end

---开局
function BilliardsProxy.Net2C_5001(data)
    EventManager.Dispatch(EventDef.E_BeginGame, data)
end

---通知击球
function BilliardsProxy.Net2C_5002(data)
    EventManager.Dispatch(EventDef.E_NoticeShotBall, data)
end

---分球消息
function BilliardsProxy.Net2C_5003(data)
    local showSeprateMsg = false
    if data then
        for _, user in pairs(data.users) do
            local player = BilliardsBattle.GetPlayerByPos(user.pos)
            if player then
                player:SetBallType(user.type)
                player:SetCanHitBalls(user.balls)
                if user.type ~= BillardsType.BillardsBallType.BLACK_BALL then  ---如果是只剩黑8了, 不弹提示
                    showSeprateMsg = true
                end
            end
        end
    end

    if showSeprateMsg then
        EventManager.Dispatch(EventDef.E_SeprateBall, data)
    end
end

----母球移动
function BilliardsProxy.Net2C_5101(data)
    BilliardsBattle.SyncFreeBallData(data)
end

---犯规消息
function BilliardsProxy.Net2C_5004(data)
    EventManager.Dispatch(EventDef.E_BreakRule, data)
end

---游戏结束
function BilliardsProxy.Net2C_5006(data)
    EventManager.Dispatch(EventDef.E_GameOver, data)
end

---客户端击球
function BilliardsProxy.Net2S_5103(timeout, pos, x, y, to_x, to_y, power, hitPosX, hitPosY)
    SendMessage(BilliardsNetDefine.Net2S_5103, {timeout = timeout, pos = pos, cue_pos = {x, y}, to_x = to_x, to_y = to_y, power = power, aiming_point = {x = hitPosX, y = hitPosY}})
end

----调整球杆角度--请求
function BilliardsProxy.ChangeCueDirAndPosReq(pos, x, y, px, py, vx, vy, ball_num, ball_vx, ball_vy, cue_dir)
    SendMessage(BilliardsNetDefine.Net2S_5102, {pos = pos, x = x, y = y, to_x = px, to_y = py, vx = vx, vy = vy, ball_num = ball_num, ball_vx = ball_vx, ball_vy = ball_vy, cue_dir = cue_dir})
end

---调整角度--返回
function BilliardsProxy.ChangeCueDirAndPosRes(data)
    BilliardsBattle.SyncCueData(data)
end

---击球结果上报
---@param fstBall number  --首次击中的球
---@param pockets table  --已落袋的球
---@param ballsPoints table --未落袋的球的位置
function BilliardsProxy.Net2S_5104(fstBall, pockets, ballsPoints)
    SendMessage(BilliardsNetDefine.Net2S_5104, {first_ball = fstBall, pocket = pockets, position = ballsPoints})
end

---服务端广播击球
function BilliardsProxy.Net2C_5103(data)
    BilliardsBattle.SyncHitBall(data)
end

---断线重连
function BilliardsProxy.Net2C_5005(data)
    EventManager.Dispatch(EventDef.E_Billiards_Reconnection, data)
end

---母球移动
---@param pos number 位置
---@param x  number --球的x坐标
---@param y  number  --球的y坐标
---@param finish number  --- 0 是继续摆放 1 是摆放完成
function BilliardsProxy.Net2S_5101(pos, x, y, finish, ball_points)
    SendMessage(BilliardsNetDefine.Net2S_5101, {pos = pos, x = x, y = y, finish = finish, ball_points = ball_points})
end

---测试摆球
function BilliardsProxy.Net2S_5106(ballPos)
    SendMessage(BilliardsNetDefine.Net2S_5106, {position = ballPos})
end

---测试摆球
function BilliardsProxy.Net2C_5106(data)
    for _, v in pairs(data.position) do
        local ballCom = BilliardsBattle.GetBall(v.num)
        if ballCom then
            ballCom:SetPosition(v.x, v.y)
        end
    end
end

return BilliardsProxy
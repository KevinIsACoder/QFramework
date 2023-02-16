local BilliardsPlayer = class("BilliardsPlayer")

function BilliardsPlayer:initialize(data)
    self.pos = data.pos
    self.user_id = data.user_id
    self.nick = data.nick
    self.headUrl = data.head

    self.ball_type = data.ball_type or 0
    self.can_hit_balls = data.can_hit_ball or {}
    self.con_enter_num = 0 --连续进球纪录
end

function BilliardsPlayer:GetPos()
    return self.pos
end

function BilliardsPlayer:GetUid()
    return self.user_id
end

function BilliardsPlayer:GetName()
    return self.nick
end

function BilliardsPlayer:GetHeadUrl()
    return self.headUrl
end

function BilliardsPlayer:IsSelf()
    return self.user_id == UserInfo.GetUserId()
end

-- 连续进球记录
function BilliardsPlayer:SetConNum(num)
    self.con_enter_num = num
end
function BilliardsPlayer:GetConNum()
    return self.con_enter_num
end

-- 更新进球记录
function BilliardsPlayer:UpdateConNum(data)
    if data.pos == self.pos then
        self.con_enter_num = data.continuous_num
    else
        self.con_enter_num = 0
    end
end

-- 可击球的类型
function BilliardsPlayer:SetBallType(ballType)
    self.ball_type = ballType
end
function BilliardsPlayer:GetBallType()
    return self.ball_type
end

-- 可击打球的列表
function BilliardsPlayer:SetCanHitBalls(ballList)
    self.can_hit_balls = ballList
end

function BilliardsPlayer:GetCanHitBalls()
    return self.can_hit_balls
end
function BilliardsPlayer:RemoveBall(num)
    for i = #self.can_hit_balls, 1, -1 do
        if self.can_hit_balls[i] == num then
            table.remove(self.can_hit_balls, i)
            break
        end
    end
end

return BilliardsPlayer